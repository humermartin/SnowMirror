using Microsoft.EntityFrameworkCore;
using MirrorRepository.Base;
using Microsoft.Extensions.DependencyInjection;
using MirrorRepository;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using log4net;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Reflection;

namespace MirrorRepoUnitTest
{

    public class SnowSyncInMemoryContext : SnowDbContext
    {
        public static readonly string INMEM_NAME = "SnowInMemory.db";
        public SnowSyncInMemoryContext()
        {
            SqlLite = true;
            Database.EnsureCreated();
        }

        public override SnowDbContext New()
        {
            return new SnowSyncInMemoryContext() { };
        }
        public override string GetConnectionString()
        {
            return "Data Source=" + INMEM_NAME;
        }
        public override void AddDesignTimeServices(IServiceCollection services)
        {
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
        }
    }

    public class SnowSyncMgntEntitiesContext : ServiceNowDbSyncMgntEntities
    {
        //protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        //protected readonly ILog DbLog = LogManager.GetLogger("DbConnection");

        public string DEFAULT_DBPWD { get; } = "SnowDbUnitTest";
        public string DBHOST { get; set; } = @".\SQLEXPRESS"; // @"ATCSW783007\SQLEXPRESS";

        public string DBNAME { get; set; } = "SnowDbMgntEntitiesUnitTest";

        public string DBUSER { get; set; } = "SnowDbUnitTest";

        public string DBPWD { protected get; set; } = "SnowDbUnitTest"; // same as Default!

        public ServiceNowDbSyncMgntEntities MgmtContext { get; set; } = new ServiceNowDbSyncMgntEntities();

        //public SnowSyncMgntEntitiesContext()
        //{
        //    Database.GetDbConnection().ConnectionString = GetConnectionString();
        //    Database.EnsureCreated();
        //}

        //protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer(GetConnectionString(), opt => opt.CommandTimeout(600));
        //    optionsBuilder.UseLoggerFactory(new Log4NetAdapterFactory(DbLog));
        //}

        public virtual string GetConnectionString()
        {
            return "Server=" + DBHOST + ";Database=" + DBNAME + ";User Id=" + DBUSER + ";Password=" + DBPWD + ";TrustServerCertificate=true";
        }

    }

}
