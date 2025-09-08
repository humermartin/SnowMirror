using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using MirrorRepository.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System.Transactions;

namespace MirrorRepository
{
    public class SnowSyncedDbContext : DbContext
    {
        SnowDbContext snowDbContext;
        public SnowSyncedDbContext(SnowDbContext ctx) {
            this.snowDbContext = ctx;
        }
        
        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            if (snowDbContext.SqlLite)
            {
                optionsBuilder.UseSqlite(snowDbContext.GetConnectionString());
            }
            else
            {
                optionsBuilder.UseSqlServer(snowDbContext.GetConnectionString(), opt => opt.CommandTimeout(600));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Entity<Model.Snow.Cmdb>().ToTable("cmdb").HasKey(c => c.SysId);
        }
        public DbSet<Model.Snow.Cmdb> cmdb { get; set; }
    }

    public class SnowDbContext : SnowDbContextBase
    {
        public virtual SnowDbContext New()
        {
            return new SnowDbContext() { DBHOST = DBHOST, DBNAME = DBNAME, 
                DBUSER = DBUSER, DBPWD = DBPWD, SqlLite = SqlLite }.Init();
        }
        public SnowDbContext Init(bool create = false)
        {
            Database.GetDbConnection().ConnectionString = GetConnectionString();
            if (create)
            {
                try
                {
                    Database.EnsureCreated();
                }
                catch (Exception e)
                {
                    Log.Info("cannot connect: " + this, e);
                    throw;
                }
            }
            return this;
        }

        public DbSet<SnowBase> SnowBases { get; set; }

        public SnowSyncedDbContext SyncedContext { get { return new SnowSyncedDbContext(this); } }

    }

    public class SnowDbContextBase : DbContext
    {
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected readonly ILog DbLog = LogManager.GetLogger("DbConnection");

        // Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
        // Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
        public string DEFAULT_DBPWD { get; } = "SnowDbUnitTest";
        public string DBHOST { get; set; } = @".\SQLEXPRESS"; // @"ATCSW783007\SQLEXPRESS";

        public string DBNAME { get; set; } = "SnowDbUnitTest";

        public string DBUSER { get; set; } = "SnowDbUnitTest";

        public string DBPWD { protected get; set; } = "SnowDbUnitTest"; // same as Default!

        public bool SqlLite { get; protected set; }
        public SnowDbContextBase()
        {
        }

        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlite("Data Source=" + INMEM_NAME); //ATCSW783007\SQLEXPRESS
            //optionsBuilder.UseSqlServer("Data Source=" + "Server=localhost;Database=" + DBNAME + ";User Id=" + DBUSER + ";Password=" + DBPWD);
            if (SqlLite)
            {
                optionsBuilder.UseSqlite(GetConnectionString());
            }
            else
            {
                optionsBuilder.UseSqlServer(GetConnectionString(), opt => opt.CommandTimeout(600));

            }
            optionsBuilder.UseLoggerFactory(new Log4NetAdapterFactory(DbLog));
        }

        public virtual string GetConnectionString()
        {
            return "Server=" + DBHOST + ";Database=" + DBNAME + ";User Id=" + DBUSER + ";Password=" + DBPWD + ";TrustServerCertificate=true";
        }

        public IServiceProvider Services => ((IInfrastructure<IServiceProvider>)this).Instance;

        public virtual ServiceProvider DesignServices
        {
            get
            {
                {
                    var services = new ServiceCollection()
                        .AddEntityFrameworkDesignTimeServices()
                        .AddSingleton<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
                    services.AddDbContextDesignTimeServices(this);
                    AddDesignTimeServices(services);
                    var provider = services.BuildServiceProvider();
                    return provider;
                }
            }
        }

        public IDatabaseModelFactory ModelFactory => DesignServices.GetRequiredService<IDatabaseModelFactory>();

        public virtual void AddDesignTimeServices(IServiceCollection services)
        {
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
        }

        public DatabaseModel CurrentModel { get {
                var databaseModelFactory = ModelFactory;
                var databaseModel = databaseModelFactory.Create(Database.GetDbConnection(), new DatabaseModelFactoryOptions());
                return databaseModel;
            } }


        public List<string> DropAllTables()
        {
            var tableNames = new List<string>();
            using (var cnx = Database.GetDbConnection())
            {
                if (cnx.State != System.Data.ConnectionState.Open) cnx.Open();

                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT table_name from INFORMATION_SCHEMA.TABLES";
                    using (var tableRead = cmd.ExecuteReader())
                        while (tableRead.Read()) tableNames.Add(tableRead.GetString(0));
                }

                DropTables(cnx, tableNames);
            }
            return tableNames;
        }

        protected void DropTables(DbConnection cnx, List<string> tableNames, int timeout = 30)
        {
            foreach (var tableName in tableNames)
            {
                if (Regex.IsMatch(tableName, "[ (),;]+")) throw new Exception("invalid parameters: ");
                using (var drop = cnx.CreateCommand())
                {
                    drop.CommandText = "DROP TABLE [" + tableName + "]";
                    drop.CommandTimeout = timeout;
                    var res = drop.ExecuteNonQuery();
                    if (res != -1)
                    {
                        throw new Exception("failed to remove: " + tableName);
                    }
                    Log.Info("executed DROP: " + tableName + " cmd: " + drop.CommandText);
                }
            }
        }

        public List<string> DropTable(string tableName, int timeout = 30)
        {
            var tableNames = new List<string>();
            using (var cnx = Database.GetDbConnection())
            {
                if (cnx.State != System.Data.ConnectionState.Open) cnx.Open();

                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT table_name from INFORMATION_SCHEMA.TABLES WHERE table_name = @tableName";
                    var par = cmd.CreateParameter();
                    par.ParameterName = "@tableName";
                    par.Value = tableName;
                    cmd.Parameters.Add(par);

                    using (var tableRead = cmd.ExecuteReader())
                        while (tableRead.Read()) tableNames.Add(tableRead.GetString(0));
                }

                DropTables(cnx, tableNames, timeout);
            }
            return tableNames;
        }

        public string Rename(string tableName, string tmpPfx)
        {
            if (Regex.IsMatch(tableName + tmpPfx, "[ (),;]+")) throw new Exception("invalid parameters: ");

            var newName = tableName.Replace(tmpPfx, "");
            var commandText = "sp_rename '" + tableName + "', '" + newName + "'";

            ExecuteSqlCommand(commandText, null);

            Log.Info("executed RENAME: " + tableName + " cmd: " + commandText);
            return newName;
        }

        public string IndexRebuild(string tableName, int commandTimeout)
        {
            var commandText = "ALTER INDEX ALL ON ["+tableName+"] REBUILD WITH (ONLINE = OFF)";
            ExecuteSqlCommand(commandText, commandTimeout);
            Log.Info("executed IndexRebuild: " + tableName + " cmd: " + commandText);
            return tableName;
        }

        /// <summary>
        /// execute stored procedure
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="loginName"></param>
        /// <param name="dbName"></param>
        /// <param name="commandTimeout"></param>
        public void ExecuteStoredProc(string commandText, string loginName, string dbName, int commandTimeout)
        {
            var ticks = DateTime.Now.Ticks;
            try
            {
                using (var cnx = Database.GetDbConnection())
                {
                    if (cnx.State != System.Data.ConnectionState.Open) cnx.Open();

                    using (var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = commandText;
                        
                        cmd.CommandTimeout = commandTimeout;
                        cmd.CommandType = CommandType.StoredProcedure;

                        var loginParameter = cmd.CreateParameter();
                        loginParameter.ParameterName = "@loginName";
                        loginParameter.Value = loginName;
                        cmd.Parameters.Add(loginParameter);

                        var dbNameParameter = cmd.CreateParameter();
                        dbNameParameter.ParameterName = "@dbName";
                        dbNameParameter.Value = dbName;
                        cmd.Parameters.Add(dbNameParameter);

                        var returnParameter = cmd.CreateParameter();
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnParameter);

                        var outputParameter = cmd.CreateParameter();
                        outputParameter.ParameterName = "@oResult";
                        outputParameter.DbType = DbType.String;
                        outputParameter.Size = 100;
                        outputParameter.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParameter);
                        
                        Log.Debug("executing stored procedure: " + commandText);
                        var res = cmd.ExecuteNonQuery();
                        var outSpId = cmd.Parameters["@oResult"].Value;
                        Log.Info($"executed stored procedure: {commandText}, returnValue: {returnParameter.Value}, output value:{outSpId}");
                        
                        if (res != -1)
                        {
                            throw new Exception("failed to exec stored procedure: " + commandText);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("failed command: " + commandText + " : " + e.Message + " : ticks=" + (DateTime.Now.Ticks - ticks));
                throw;
            }
        }

        public void ExecuteSqlCommand(string commandText, int? commandTimeout)
        {
            var ticks = DateTime.Now.Ticks;
            try
            {
                using (var cnx = Database.GetDbConnection())
                {
                    if (cnx.State != System.Data.ConnectionState.Open) cnx.Open();

                    using (var rename = cnx.CreateCommand())
                    {
                        rename.CommandText = commandText;
                        if (commandTimeout != null)
                        {
                            rename.CommandTimeout = (int)commandTimeout;
                        }
                        Log.Debug("executing SQL: " + commandText);
                        var res = rename.ExecuteNonQuery();
                        Log.Info("executed SQL: " + commandText + ", res=" + res);
                        if (res != -1)
                        {
                            throw new Exception("failed to exec: " + commandText);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("failed command: " + commandText + " : " + e.Message + " : ticks=" + (DateTime.Now.Ticks-ticks));
                throw;
            }
        }

        public override string ToString()
        {
            return GetType().Name + "=" + DBUSER + "@" + DBHOST + ":"+DBNAME + StateString;
        }
        string StateString { get { try { return "(con=" + Database.GetDbConnection().State + ")"; } catch (Exception e) { return e.ToString(); } } }
    }
}
