using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Helpers;
using MirrorRepository.Model;
using MirrorRepository.REST;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using MirrorRepository.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using MirrorRepository.Model.SyncParams;
using MirrorRepository.Processor;
using Microsoft.Data.SqlClient;

namespace MirrorRepository
{
    /// <summary>
    /// The process executing the synchronization.
    /// </summary>
    public class SnowProcessor : SnowProcessorBase
    {
        public static readonly string TMP_TABLE_PFX = "TMP_TABLE_";


        public RestClient NewRestClient()
        {
            var baseUrl = "https://" + SnowAccessSettings.Servername // Port != null WTF :-/
                + (SnowAccessSettings.Port > 16 ? ":" + SnowAccessSettings.Port : ""); // "https://a1int.service-now.com"
            if (SnowAccessSettings.ProxyServer != null)
            {
                RestClient.A1_PROXY = "http://" + SnowAccessSettings.ProxyServer + ":" + (SnowAccessSettings.ProxyPort ?? 8080) + "/";
            }
            if (SnowAccessSettings.ProxyUserName != null)
            {
                RestClient.ProxyCredentials = new NetworkCredential(SnowAccessSettings.ProxyUserName, SnowAccessSettings.ProxyUserPassword);
            }

            return RestClient.Build(baseUrl, SnowAccessSettings.UserName, SnowAccessSettings.Password);
        }

        public EsbClient NewEsbClient()
        {
            var baseUrl = EsbAccessSettings.Endpoint;
            //if (EsbAccessSettings.ProxyUserName != null)
            //{
            //    RestClient.ProxyCredentials = new NetworkCredential(EsbAccessSettings.ProxyUserName, EsbAccessSettings.ProxyUserPassword);
            //}

            return EsbClient.Build(baseUrl, EsbAccessSettings.User, EsbAccessSettings.Password);
        }

        /// <summary>
        /// Execute the Synchronization
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Process()
        {
            Log.Info("Processing: " + this);
            LogMessages.Add(new LogMessage { Key = "", Table = "", Message = "started: " + SyncScheduler.SnowTableNames });

            bool kafkaSync = EsbAccessSettings != null && EnumTargetType.Kafka.ToString().Equals(EsbAccessSettings.TargetType);
            
            lock (this)
            {
                if (SnowDictionary == null) SnowDictionary = ReadDictionary();
            }

            if (!kafkaSync && SyncType == SyncProcessType.Full)
            {
                TableNamePrefix = TMP_TABLE_PFX;
                Migration = new SnowMigration() { SnowTablePrefix = TableNamePrefix }.Init(Context.New());
            }

            //var comp = new Comparer<SnowTables>{ public override Equals(t1, t2) => { t1.Name == t2.Name};
            SyncTaskFactory = new TaskFactory(new SnowSyncTaskScheduler(SyncScheduler.MaxThreads));
            foreach (var table in SyncScheduler.SnowTables.Where(t => !string.IsNullOrEmpty(t.Name)).DistinctBy(t => t.Name))
            {
                //is table enabled for sync
                var tblSyncEnabled = kafkaSync || IsTableSyncEnabled(table.Name, SyncScheduler.SynchronizationId) || ForceSync;
                if (tblSyncEnabled == false)
                {
                    Log.Info($"skip synchronization for table: {table.Name}. table is disabled.");
                    continue;
                }

                if (ForceSync) // for testing!
                {
                    SyncScheduler.ThreadsPerTable = 1;
                }
                
                SyncTask syncTask;
                if (kafkaSync && (SyncScheduler.KafkaMode.Equals(EnumKafkaMode.SqlToKafka.ToString()) || SyncScheduler.KafkaMode == null))
                {
                    SyncScheduler.AutoSchemaUpdate = false;
                    SyncType = SyncProcessType.Delta;
                    syncTask = new KafkaTask()
                    {
                        Table = table,
                        PageSize = SyncScheduler.PageSize,
                        MaxErrorsPerPage = SyncScheduler.MaxErrorsPerPage,
                        PoolSize = SyncScheduler.ThreadsPerTable,
                        SleepTime = SyncScheduler.ThreadSleepTime,
                        RequestTimeout = SyncScheduler.RequestTimeout,
                        Processor = this,
                        Client = NewEsbClient(),
                        Context = Context.New(),
                        SyncType = SyncType,
                        SynchronizationId = SyncScheduler.SynchronizationId ?? Guid.Empty,
                        Invocation = Invocation,
                        ExecuteCleanup = SyncScheduler.ExecuteCleanup,
                        KafkaBlockSize = SyncScheduler.KafkaBlockSize.GetValueOrDefault(50),
                        ParentThreadId = Thread.CurrentThread.ManagedThreadId,
                    };

                } 
                else if (kafkaSync && !string.IsNullOrWhiteSpace(SyncScheduler.KafkaMode) && SyncScheduler.KafkaMode.Equals(EnumKafkaMode.SnowToKafka.ToString()))
                {
                    syncTask = new SnowToKafkaTask()
                    {
                        Table = table,
                        PageSize = SyncScheduler.PageSize,
                        MaxErrorsPerPage = SyncScheduler.MaxErrorsPerPage,
                        PoolSize = SyncScheduler.ThreadsPerTable,
                        SleepTime = SyncScheduler.ThreadSleepTime,
                        RequestTimeout = SyncScheduler.RequestTimeout,
                        Processor = this,
                        Client = NewRestClient(),
                        EsbClient = NewEsbClient(),
                        Context = Context.New(),
                        SyncType = SyncType,
                        SynchronizationId = SyncScheduler.SynchronizationId ?? Guid.Empty,
                        Invocation = Invocation,
                        ExecuteCleanup = SyncScheduler.ExecuteCleanup,
                        KafkaBlockSize = SyncScheduler.KafkaBlockSize.GetValueOrDefault(50),
                        ParentThreadId = Thread.CurrentThread.ManagedThreadId,
                    };
                }
                else 
                {

                    syncTask = new SnowTask()
                    {
                        Table = table,
                        PageSize = SyncScheduler.PageSize,
                        MaxErrorsPerPage = SyncScheduler.MaxErrorsPerPage,
                        PoolSize = SyncScheduler.ThreadsPerTable,
                        SleepTime = SyncScheduler.ThreadSleepTime,
                        RequestTimeout = SyncScheduler.RequestTimeout,
                        Processor = this,
                        Client = NewRestClient(),
                        Context = Context.New(),
                        SyncType = SyncType,
                        SynchronizationId = SyncScheduler.SynchronizationId ?? Guid.Empty,
                        Invocation = Invocation,
                        ExecuteCleanup = SyncScheduler.ExecuteCleanup,
                        ParentThreadId = Thread.CurrentThread.ManagedThreadId,
                    };
                }

                try
                {
                    Migrate(table, syncTask.GetKey());
                }
                catch (Exception e)
                {
                    LogError("Schema update failed for table " + table.Name + " : " + e.Message, e, syncTask.GetKey(), table);
                    //throw e;
                }

                if (SyncScheduler != null)
                {
                    if (SyncScheduler.IntervalInMinutes != null)
                    {
                        syncTask.IntervalInMinutes = Convert.ToInt32(SyncScheduler.IntervalInMinutes);
                    }
                }

                SyncTasks.Add(table.Name, new TaskEntry { SyncTask = syncTask, Task = SyncTaskFactory.StartNew(syncTask.Execute) });

                LogMessages.Add(new LogMessage { Key = "", Table = table.Name, Message = "started: " + table.Name });

                if (table.Columns != null && table.Columns.Any())
                {
                    Log.Info($"started: {SyncName} for: {table.Name}, columns: {String.Join(", ", table.Columns.ToArray())}");
                }
                else
                {
                    Log.Info("started: " + SyncName + " for: " + table.Name);
                }
                
                
            }
            Task.WaitAll(SyncTasks.Values.Select(e => e.Task).ToArray());

            //reset custom delta start at the end of whole delta synchronization
            if (SyncType == SyncProcessType.Delta)
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var sync = ctx.Synchronization.FirstOrDefault(s => s.Id == SyncScheduler.SynchronizationId);

                    if (sync != null && sync.CustomDeltaStart != null)
                    {
                        sync.CustomDeltaStart = null;
                        ctx.SaveChanges();
                    }
                }
            }

            List<TaskEntry> failed = SyncTasks.Values.Where(t => t.Task.IsFaulted).ToList();
            if (failed.Count > 0)
            {
                failed.ForEach(e =>
                {
                    LogMessages.Add(new LogMessage { Key = e.SyncTask.GetKey(), Table = e.SyncTask.Table.Name, Message = e.Task.Exception.Message });
                    Log.Info("failed: " + SyncName + " for: " + e.SyncTask.Table.Name + ", msg=" + e.Task.Exception.Message);
                });
                throw failed.First().Task.Exception;
            }
            
            LogMessages.Add(new LogMessage { Key = "", Table = "", Message = "ended: " + SyncScheduler.SnowTableNames });
            Log.Info("ended: " + SyncName + " for: " + string.Join(",", SyncTasks.Keys) + " Start: " + Start + ", Minutes: " + (DateTime.Now - Start).TotalMinutes);
        }

        /// <summary>
        /// Is sync enabled for table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="syncId"></param>
        /// <returns></returns>
        public bool IsTableSyncEnabled(string tableName, Guid? syncId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tableName) && syncId != Guid.Empty && syncId != null)
                {
                    using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                    {
                        var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == syncId);
                        if (synchronization == null) return false;

                        SnowTableDefinition tableDefinitionEntity = snowEntities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(tableName));

                        if (tableDefinitionEntity != null && !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                        {
                            List<TableParam> tblParams = JsonConvert.DeserializeObject<List<TableParam>>(tableDefinitionEntity.TableParams);
                            var instance = snowEntities.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                            var syncType = snowEntities.SyncType.FirstOrDefault(s => s.Id == synchronization.SyncTypeId);

                            var tableParam = tblParams?.FirstOrDefault(t => t.InstanceName == instance?.InstanzName);
                            if (tableParam != null)
                            {
                                SyncParameter syncParams = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeName == syncType?.TypeName)?.SyncParameter;

                                if (syncParams?.Enabled != null && syncParams.Enabled == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate Schema update commands for the Synchronization
        /// </summary>
        public IReadOnlyList<MigrationCommand> GenerateCommands()
        {
            lock (this)
            {
                if (SnowDictionary == null) SnowDictionary = ReadDictionary();
            }
            var commands = new List<MigrationCommand>();

            SyncTaskFactory = new TaskFactory(new SnowSyncTaskScheduler(SyncScheduler.MaxThreads));
            return GenerateCommands(SyncScheduler.SnowTables.Where(t => !string.IsNullOrEmpty(t.Name)).DistinctBy(t => t.Name).Select(t=>t.Name));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected Dictionary<SnowDictEntry, List<SnowDictEntry>>  ReadDictionary()
        {
            try
            {
                var rc = NewRestClient();

                var entries = new List<SnowDictEntry>();
                foreach (var table in SyncScheduler.SnowTables)
                {
                    var content = rc.Read("/api/now/table/sys_dictionary", SnowParms.New.equals(PROP.name, table.Name));

                    var dict = rc.Deserialize<DictionaryResponse>(content);
                    entries.AddRange(dict.result);
                }

                var dictionary = rc.ToTables(entries);
                Log.Info("read dictionary: " + string.Join(",", dictionary.Keys.ToList()));
                return dictionary;
            }
            catch (Exception e)
            {
                Log.Info("failed to read dictionary", e);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Migrate(SnowTables table, string key)
        {
            if (SyncType == SyncProcessType.Full)
            {
                var tmpTable = TableNamePrefix + table.Name;
                Context.New().DropTable(tmpTable);
                LogMessages.Add(new LogMessage { Key = key, Table = table.Name, Message = "Dropped: " + tmpTable });
            }

            if (SyncScheduler.AutoSchemaUpdate || SyncType == SyncProcessType.Full)
            {
                MigrateDatabase(table, key);
            } else
            {
                Migration = new SnowMigration() { SnowTablePrefix = TableNamePrefix, SyncName = SyncName }.Init(Context.New());
                Log.Info("not migrated[" + SyncName + "]: "+ Migration + ", key=" + key);
            }
            Log.Info("Migrated[" + SyncName + "]: " + Migration + ", key=" + key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void MigrateDatabase(SnowTables table, string key)
        {
            try
            {
                var ctx = Context.New();
                Migration = new SnowMigration() { SnowTablePrefix = TableNamePrefix, SyncName = SyncName };

                var mig = Migration.Migrate(ctx, SnowDictionary.Where(d => d.Key.name == table.Name)
                    .ToDictionary(d => d.Key, d => d.Value));

                Migration = new SnowMigration(){ SnowTablePrefix = TableNamePrefix, SyncName = SyncName }.Init(Context.New());

                LogMessages.Add(new LogMessage { Key = key, Table = table.Name, Message = "Migrated: " + table.Name });
                Log.Info("Migrated["+SyncName+"]: commands: " + mig.Commands.Count + " for tables: " + table.Name + ", key=" + key);
            }
            catch (Exception e)
            {
                Log.Warn("Failed["+SyncName+"] to migrate " + table.Name + ", key=" + key, e);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IReadOnlyList<MigrationCommand> GenerateCommands(IEnumerable<string> tableNames)
        {
            var ctx = Context.New();
            var migration = new SnowMigration() { SnowTablePrefix = TableNamePrefix };

            migration.GenerateCommands(ctx, SnowDictionary.Where(d => tableNames.Contains(d.Key.name))
                .ToDictionary(d => d.Key, d => d.Value));
            
            return migration.Commands;
        }

        /// <summary>
        /// Retry sync process for selected tables
        /// </summary>
        /// <param name="syncId"></param>
        /// <param name="snowTables"></param>
        /// <param name="userShortName"></param>
        public void RetrySyncProcess(Guid syncId, List<SnowTables> snowTables, string userShortName)
        {
            if (syncId != Guid.Empty && snowTables.Any())
            {
                SyncSchedulerModel model = new SyncSchedulerModel();
                var synchronization = model.FindInternal<Data.SnowDbSyncMgnt.Synchronization>(syncId);
                model.Init(synchronization);

                if (!string.IsNullOrWhiteSpace(userShortName))
                {
                    try
                    {
                        var tables = String.Join(", ", snowTables.Select(x => x.Name));
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User: {userShortName} started synchronization: {synchronization.Name} with following table(s): {tables}");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.InnerException}");
                    }
                }
                
                var snowExec = new SnowProcessorRunner()
                {
                    SynchronizationId = syncId,
                    Invocation = EnumInvocation.Manual
                };
                snowExec.RunAsync(snowTables);
            }
        }

        /// <summary>
        /// Gets the sql table row count
        /// </summary>
        /// <param name="synchronization"></param>
        /// <param name="entities"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public int GetSqlRowCount(Data.SnowDbSyncMgnt.Synchronization synchronization, ServiceNowDbSyncMgntEntities entities, SnowTables table)
        {
            try
            {
                var mirrorDb = entities.DatabaseSettings.FirstOrDefault(i => i.Id == synchronization.DatabaseSettingsId);

                if (mirrorDb == null) return 0;

                byte[] data = Convert.FromBase64String(mirrorDb.Password);
                string mirrorDbPwd = System.Text.Encoding.UTF8.GetString(data);

                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = $@"Server={mirrorDb.Servername},{mirrorDb.Port};Database={mirrorDb.Databasename};TrustServerCertificate=True;User Id={mirrorDb.Username};Password={mirrorDbPwd}";

                using (SqlCommand cmd = new SqlCommand("GetRowCountFromTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@tableName", SqlDbType.VarChar).Value = table.Name;
                    cmd.Parameters.Add("@rowCount", SqlDbType.Int).Direction = ParameterDirection.Output;

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    int rowCount = Convert.ToInt32(cmd.Parameters["@rowCount"].Value);
                    
                    return (int)rowCount;
                }
                
                
            } 
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name} - Could not get SQL RowCount for table = {table.Name}. {e.InnerException}");
            }

            return 0;
        }

        public override string ToString()
        {
            return GetType().Name + "[" + SyncScheduler + "]: acc=" + SnowAccessSettings + ", dbCtx=" + Context + ", inv=" + Invocation.ToString();
        }
    }

}
