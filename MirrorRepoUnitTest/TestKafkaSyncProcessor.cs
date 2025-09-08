using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository.Model;
using MirrorRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorRepository.Model.Snow;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class TestKafkaSyncProcessor : SyncProcessorTestBase
    {
        [TestMethod]
        public void TestKafkaProcessUpdate()
        {
            var snowDbContextToDrop = new SnowDbContext() { DBNAME = "SnowDbKafkaUnitTest" }.Init();
            snowDbContextToDrop.DropAllTables();
            
            var snowDbContext = new SnowDbContext() { DBNAME = "SnowDbKafkaUnitTest" }.Init(true);
            var ctx = InitDB(snowDbContext, true, DICTIONARY_FILE_FULL);

            var SnowAccessSettings = SnowAccess();

            List<SnowTables> tables = new List<SnowTables>();
            tables.Add(new SnowTables() { Name = "cmdb" });
            tables.Add(new SnowTables() { Name = "incident" });
            tables.Add(new SnowTables() { Name = "cmdb_ci_apache_web_server" });

            var SyncScheduler = new SyncSchedulerModel() { AutoSchemaUpdate = true, RequestTimeout = 120 };
            SyncScheduler.SynchronizationId = currentSynchronization.Id;
            SyncScheduler.SnowTables.AddRange(tables);

            var proc = new SnowProcessor() {
                Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings,
                SyncScheduler = SyncScheduler,
                SyncType = MirrorRepository.Processor.SyncProcessType.Full,
            };
            proc.ForceSync = true;
            proc.Process();

            SyncScheduler = new SyncSchedulerModel() { AutoSchemaUpdate = false, RequestTimeout = 120 };
            SyncScheduler.SynchronizationId = currentSynchronization.Id;
            SyncScheduler.SnowTables.AddRange(tables);

            proc = new SnowProcessor()
            {
                Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings,
                SyncScheduler = SyncScheduler,
                SyncType = MirrorRepository.Processor.SyncProcessType.Delta,
            };
            proc.ForceSync = true;
            proc.Process();

            SyncScheduler = new SyncSchedulerModel() { RequestTimeout = 120 };
            SyncScheduler.SynchronizationId = currentSynchronization.Id;

            //SyncScheduler.SnowTables.Add(new SnowTables() { Name = "cmdb" });
            //SyncScheduler.SnowTables.Add(new SnowTables() { Name = "sc_request" });
            SyncScheduler.SnowTables.AddRange(tables);

            proc = new SnowProcessor()
            {
                SyncName = currentSynchronization.Name,
                ForceSync = true,   // TESTING!!!
                Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings,
                SyncScheduler = SyncScheduler,
                EsbAccessSettings = currentSynchronization.SyncTarget,
            };
            proc.Process();
        }

        [Ignore] // encrypt/decrypt password missing
        [TestMethod]
        public void TestKafkaProcessorRunnerUpdate()
        {
            var snowDbContextToDrop = new SnowDbContext() { DBNAME = "SnowDbKafkaUnitTest" }.Init();
            snowDbContextToDrop.DropAllTables();

            var snowDbContext = new SnowDbContext() { DBNAME = "SnowDbKafkaUnitTest" }.Init(true);
            var ctx = InitDB(snowDbContext, true);

            var SnowAccessSettings = SnowAccess();

            var SyncScheduler = new SyncSchedulerModel() { AutoSchemaUpdate = true };
            SyncScheduler.SynchronizationId = currentSynchronization.Id;
            SyncScheduler.SnowTables.Add(new SnowTables() { Name = "cmdb" });

            var proc = new SnowProcessor() { Context = ctx.New(), SnowAccessSettings = SnowAccessSettings, SyncScheduler = SyncScheduler };
            proc.ForceSync = true;
            proc.Process();

            SyncScheduler = new SyncSchedulerModel();
            SyncScheduler.SynchronizationId = currentSynchronization.Id;

            SnowTables snowTable = new SnowTables() { Name = "cmdb" };
            SyncScheduler.SnowTables.Add(snowTable);

            proc = new SnowProcessor()
            {
                SyncName = currentSynchronization.Name,
                ForceSync = true,   // TESTING!!!
                Context = ctx.New(),
                SnowAccessSettings = SnowAccessSettings,
                SyncScheduler = SyncScheduler,
                EsbAccessSettings = currentSynchronization.SyncTarget,
            };
            
            var procRunner = new SnowProcessorRunner() { SynchronizationId = currentSynchronization.Id };
            procRunner.Run(SyncScheduler.SnowTables);
        }
    }
}
