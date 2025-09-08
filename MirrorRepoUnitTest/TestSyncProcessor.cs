using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model;
using MirrorRepository.Model.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using MirrorRepository.Enums;
using MirrorRepository.Model.SyncParams;
using MirrorRepository.Processor;
using MirrorRepository.REST;
using static MirrorRepoUnitTest.TestSyncProcessor;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class TestSyncProcessor : SyncProcessorTestBase
    {
        //[Ignore]
        [TestMethod]
        public void TestSyncProcess()
        {
            var ctx = InitDB(true);

            var SnowAccessSettings = SnowAccess();

            var SyncScheduler = new SyncSchedulerModel() { AutoSchemaUpdate = true };
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "cmdb" });

            var proc = new SnowProcessor() { Context = ctx.New(), SnowAccessSettings = SnowAccessSettings, SyncScheduler = SyncScheduler};
            proc.Process();

        }

        //[Ignore]
        [TestMethod]
        public void TestSyncProcessUpdate()
        {
            var ctx = InitDB(false);

            var SnowAccessSettings = SnowAccess();

            var SyncScheduler = new SyncSchedulerModel();
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "cmdb" });

            var proc = new SnowProcessor() { Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings, 
                SyncScheduler = SyncScheduler,
            };
            proc.Process();

        }

        //[Ignore]
        [TestMethod]
        public void TestSyncProcessFull()
        {
            var ctx = InitDB(true);

            var SnowAccessSettings = SnowAccess();

            var SyncScheduler = new SyncSchedulerModel();
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "u_a1_cd_orgunit" });
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "azure_service_principal" });

            var proc = new SnowProcessor()
            {
                Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings,
                SyncScheduler = SyncScheduler,
                SyncType = SyncProcessType.Full,
            };
            proc.Process();

        }

        [TestMethod]
        public void TestGenerateCommands()
        {
            var ctx = InitDB(true);

            var SnowAccessSettings = SnowAccess();

            var SyncScheduler = new SyncSchedulerModel() { AutoSchemaUpdate = true };
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "cmdb" });

            var proc = new SnowProcessor() { Context = ctx.New(), SnowAccessSettings = SnowAccessSettings, SyncScheduler = SyncScheduler };
            var cmds = proc.GenerateCommands();

            Assert.IsNotNull(cmds);

        }

        //[Ignore]
        [TestMethod]
        public void TestSnowProcessorRunnerAsService()
        {
            var ctx = new SnowDbContext();
            var runner = new SnowProcessorRunner();
            runner.RunAsService("SnowToKafkaService");//SnowToKafkaService, MirrorService
            if (!runner.ServiceSnowProcessorRunner.ProcessorTask.IsCompleted)
            {
                runner.ServiceSnowProcessorRunner.ProcessorTask.Wait();
            }
        }

        [TestMethod]
        public void TestDeserializer()
        {
           

            JsonSerializer s = new JsonSerializer();
            //var result =  s.Deserialize(new JsonTextReader(new StringReader(text)));
            using (StreamReader r = new StreamReader(@"testfiles\cmdb_ci.json"))
            {
                //var result =  s.Deserialize(new JsonTextReader(new StringReader(text)));
                string json = r.ReadToEnd();
                var response = s.Deserialize<QueryResponse>(new JsonTextReader(new StringReader(json)));
            }
        }

        [TestMethod]
        public void TestDateTimeFormats()
        {
            DateTime twoam = SnowBase.ParseTime("23:45");
            Assert.AreEqual(23, twoam.Hour);
            Assert.AreEqual(45, twoam.Minute);
            Assert.AreEqual(DateTime.Now.Date, twoam.Date);

            DateTime lastDay2020lastSecond = SnowBase.ParseDateTime("31.12.2020 23:59:59");
            Assert.AreEqual(23, lastDay2020lastSecond.Hour);
            Assert.AreEqual(59, lastDay2020lastSecond.Minute);
            Assert.AreEqual(59, lastDay2020lastSecond.Second);
            Assert.AreEqual(31, lastDay2020lastSecond.Day);
            Assert.AreEqual(12, lastDay2020lastSecond.Month);
            Assert.AreEqual(2020, lastDay2020lastSecond.Year);

            DateTime lastDay2020lastMinute = SnowBase.ParseDateTime("31.12.2020 23:59");
            Assert.AreEqual(23, lastDay2020lastMinute.Hour);
            Assert.AreEqual(59, lastDay2020lastMinute.Minute);
            Assert.AreEqual(31, lastDay2020lastMinute.Day);
            Assert.AreEqual(12, lastDay2020lastMinute.Month);
            Assert.AreEqual(2020, lastDay2020lastMinute.Year);
        }



        [Ignore]
        [TestMethod]
        public void TestIndexRebuildTimeout()
        {
            SnowDbContext ctx = new SnowDbContext();
            ctx.DBNAME = "ServiceNowDbSync_KIT";
            ctx.DBHOST = "VMSNWQP160\\ICTS_SNOW_DWH";
            ctx.DBUSER = "dwh_sync";
            ctx.DBPWD = "*********";

            var db = ctx.New();
            db.IndexRebuild("cmdb_ci", 1000);
        }

        [TestMethod]
        public void TestGetAppSettings()
        {
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();

            var snowInheritTableModel = appSettingsModel.InheritanceSettings;

            foreach (var parentChildModel in snowInheritTableModel)
            {
                var parentTable = parentChildModel.TableName;
                var childTable = parentChildModel.SnowTableChildren;
            }

        }

        [TestMethod]
        public void TestProcessorStart()
        {
            using (ServiceNowDbSyncMgntEntities mgntDb = new ServiceNowDbSyncMgntEntities())
            {
                bool shouldRun = false;

                var sync = mgntDb.Synchronization.FirstOrDefault(i => i.Name == "TaskSla_Full");

                var timeToStartToday = SnowBase.ParseTime(sync.SyncStartTime);

                DateTime now = DateTime.Now;
                DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                TimeSpan timeOfDay = timeToStartToday.TimeOfDay;
                firstDayOfMonth = firstDayOfMonth.Date + timeOfDay;
                if (sync.ServiceStartDate.HasValue)
                {
                    int startDateMonth = sync.ServiceStartDate.Value.Month;
                    if (DateTime.Now > firstDayOfMonth && now.Month != startDateMonth)
                    {
                        shouldRun = true;
                        Assert.IsTrue(shouldRun);
                    }
                }
            }

            
            
        }
        [Ignore]
        [TestMethod]
        public void TestSelectLocalCMDB()
        {
            var ctx = InitDB();
            var sysIds = from c in ctx.SyncedContext.cmdb
                         where c.SysUpdatedOn < DateTime.Now
                         select c.SysId
                         .Take(100)
                         .ToList();
            Assert.IsNotNull(sysIds);
            Assert.IsTrue(sysIds.Count() > 0);
            Assert.Equals(sysIds.Count(),100);
        }

        [TestMethod]
        public void TestTableParams()
        {
            TableParam tblParams = new TableParam();
            var initTabelDefintions = tblParams.Init();

            
            
        }

    }

    public class SyncProcessorTestBase
    {
        public const string DICTIONARY_FILE_DEFAULT = "sys_dictionary_cmdb.json";
        public const string DICTIONARY_FILE_FULL = "sys_dictionary_full.json";
        public readonly Guid DATABASE_SETTINGS_GUID = Guid.Parse("12345678-ABCD-1234-ABCD-123456789000");
        public readonly Guid SYNCHRONIZATION_GUID = Guid.Parse("12345678-ABCD-1234-ABCD-123456789000");
        public readonly Guid SYNC_TARGET_GUID = Guid.Parse("12345678-ABCD-1234-ABCD-123456789000");

        public Synchronization currentSynchronization;

        public virtual SnowDbContext InitDB(bool delete = false)
        {
            SnowDbContext ctx = new SnowSyncInMemoryContext();
            return InitDB(ctx, delete);
        }

        public virtual SnowDbContext InitDB(SnowDbContext ctx, bool delete = false, string dictionaryFile = DICTIONARY_FILE_DEFAULT)
        {

            if (delete)
            {
                if (File.Exists(SnowSyncInMemoryContext.INMEM_NAME))
                    File.Delete(SnowSyncInMemoryContext.INMEM_NAME);
            }

            dynamic entity = new SnowBase { sys_id_str = "abcd1234abcd1234abcd1234abcd1234" };
            entity.DynProp = "DynProp";

            //((object)entity).GetType().GetProperty("test").SetValue(entity, "test");
            string idStr = entity.sys_id_str;
            var sbes = (from sb in ctx.SnowBases where sb.sys_id_str == idStr select sb).ToList();
            if (sbes.Count == 0)
            {
                ctx.SnowBases.Add(entity);
                ctx.SaveChanges();
            }

            var dict = getDict(dictionaryFile);
            var mig = new SnowMigration().Migrate(ctx, dict);
            //var mig = new SnowMigration().Migrate(ctx, null); // getDict(schemaFileName)); // getDictCMDB()

            sbes = (from sb in ctx.SnowBases where sb.sys_id_str == idStr select sb).ToList();
            if (sbes.Count == 0)
            {
                ctx.SnowBases.Add(entity);
                ctx.SaveChanges();
            }

            //ServiceNowDbSyncMgntEntities mgnt = new SnowSyncMgntEntitiesContext();
            ServiceNowDbSyncMgntEntities mgnt = new ServiceNowDbSyncMgntEntities();

            Synchronization sync = mgnt.Synchronization.Where(s => SYNCHRONIZATION_GUID.Equals(s.Id)).FirstOrDefault();
            if (sync == null)
            {
                sync = new Synchronization() { Id = SYNCHRONIZATION_GUID, Name = "KafkaTest" };
                mgnt.Synchronization.Add(sync);
            }
            sync.SyncInterval = EnumInterval.Manual.ToString();

            DatabaseSettings ds = mgnt.DatabaseSettings.Where(s => DATABASE_SETTINGS_GUID.Equals(s.Id)).FirstOrDefault();
            if (ds == null)
            {
                ds = new DatabaseSettings();
                mgnt.DatabaseSettings.Add(ds);
            }
            ds.Id = DATABASE_SETTINGS_GUID;
            ds.Instancename = "KafkaTest";
            ds.Databasename = "KafkaTest";
            ds.Schemaname = "KafkaTest";
            ds.Servername = ctx.DBHOST;
            ds.Username = ctx.DBUSER;
            ds.Password = ctx.DEFAULT_DBPWD;

            SyncTarget target = mgnt.SyncTarget.Where(s => SYNC_TARGET_GUID.Equals(s.Id)).FirstOrDefault();
            if (target == null)
            {
                target = new SyncTarget();
                mgnt.SyncTarget.Add(target);
            }
            target.Id = SYNC_TARGET_GUID;
            target.Targetname = "Test";
            target.TargetType = EnumTargetType.Kafka.ToString();
            target.User = "test";
            target.Password = "test";
            target.Endpoint = "http://localhost:8080/servicenow-esb-simulator/rs/simulator";

            sync.DatabaseSettings = ds;
            sync.SyncTarget = target;

            mgnt.SaveChanges();

            currentSynchronization = sync;

            return ctx;
        }

        public virtual Dictionary<SnowDictEntry, List<SnowDictEntry>> getDict(string jsonDictionaryFile = "dictionary_LIKEu_.json")
        {
            string content = File.ReadAllText(Path.Combine("testfiles", jsonDictionaryFile));
            var rc = new RestClient();
            DictionaryResponse dictionaryResponse = rc.Deserialize<DictionaryResponse>(content);
            return rc.ToTables(dictionaryResponse.result.ToList());
        }

        public virtual InstanzSettings SnowAccess()
        {
            var SnowAccessDatabaseSettings = new InstanzSettings();
            SnowAccessDatabaseSettings.UserName = TestRestClient.USER;
            SnowAccessDatabaseSettings.Password = TestRestClient.PWD;
            SnowAccessDatabaseSettings.Servername = TestRestClient.HOST;
            SnowAccessDatabaseSettings.ProxyServer = TestRestClient.PROXYHOST;
            SnowAccessDatabaseSettings.ProxyPort = TestRestClient.PROXYPORT;
            SnowAccessDatabaseSettings.ProxyUserName = TestRestClient.PROXYUSER;
            SnowAccessDatabaseSettings.ProxyUserPassword = TestRestClient.PROXYPASS;
            SnowAccessDatabaseSettings.Port = !string.IsNullOrEmpty(TestRestClient.PORT) ? Convert.ToInt32(TestRestClient.PORT) : 443;
            return SnowAccessDatabaseSettings;
        }

    }
}
