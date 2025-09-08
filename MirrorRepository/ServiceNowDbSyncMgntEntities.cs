//------------------------------------------------------------------------------
//     This code was COPIED from generated from a template.
//     Original code in MirrorRepository\Data\SnowDbSyncMgnt\SnowDbSyncMgntModel.Context.cs
//------------------------------------------------------------------------------

using System.Configuration;
using log4net;
using Microsoft.EntityFrameworkCore;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository
{
    //using System.Data.Entity;

    public class ServiceNowDbSyncMgntEntities : ServiceNowDbSyncMgntEntitiesBase
    {
    }

    public class ServiceNowDbSyncMgntEntitiesBase : DbContext
    {
        protected readonly ILog DbLog = LogManager.GetLogger("DbConnection");
        readonly Log4NetAdapterFactory loggerFactory;
        public bool UseOverride { get; set; } = true;
        public bool EnsureCreated { get; set; }

        public ServiceNowDbSyncMgntEntitiesBase()
        //            : base("name=ServiceNowDbSyncMgntEntities")
        {
            loggerFactory = new Log4NetAdapterFactory(DbLog);
            if (UseOverride) loggerFactory.LevelOverride = Microsoft.Extensions.Logging.LogLevel.Warning;
            if (EnsureCreated)
            {
                Database.EnsureCreated();
            }
            if (DbLog.IsDebugEnabled)
            {
                loggerFactory.LevelOverride = null;
            }
        }

        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(GetConnectionString(), opt => opt.CommandTimeout(600));
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        public virtual string GetConnectionString()
        {
            //ConfigurationSection config = System.Configuration.Configuration.GetSection("AppSettings");
            return ConfigurationManager.ConnectionStrings["ServiceNowDbSyncMgntEntities"].ConnectionString;
        }

        //public IServiceProvider Services => ((IInfrastructure<IServiceProvider>)this).Instance;

        //public virtual ServiceProvider DesignServices
        //{
        //    get
        //    {
        //        {
        //            var services = new ServiceCollection()
        //                .AddEntityFrameworkDesignTimeServices()
        //                .AddSingleton<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
        //            services.AddDbContextDesignTimeServices(this);
        //            AddDesignTimeServices(services);
        //            var provider = services.BuildServiceProvider();
        //            return provider;
        //        }
        //    }
        //}

        //public IDatabaseModelFactory ModelFactory => DesignServices.GetRequiredService<IDatabaseModelFactory>();

        //public virtual void AddDesignTimeServices(IServiceCollection services)
        //{
        //    new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
        //}


        public virtual DbSet<DatabaseSettings> DatabaseSettings { get; set; }

        public virtual DbSet<InstanzSettings> InstanzSettings { get; set; }

        public virtual DbSet<AppSettings> AppSettings { get; set; }

        public virtual DbSet<ServiceSettings> ServiceSettings { get; set; }

        public virtual DbSet<SyncProcess> SyncProcess { get; set; }

        public virtual DbSet<SyncType> SyncType { get; set; }

        public virtual DbSet<SyncTarget> SyncTarget { get; set; }

        public virtual DbSet<Data.SnowDbSyncMgnt.Synchronization> Synchronization { get; set; }

        public virtual DbSet<ManagementRole> ManagementRole { get; set; }

        public virtual DbSet<Principals> Principals { get; set; }

        public virtual DbSet<NotificationHistory> NotificationHistory { get; set; }

        public virtual DbSet<SnowTableDefinition> SnowTableDefinition { get; set; }

        public virtual DbSet<TableMonitoring> TableMonitoring { get; set; }

    }
}
