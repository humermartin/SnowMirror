using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository;
using MirrorRepository.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepoUnitTest
{


    [TestClass]
    public class TestModelFactorySqlServer : TestModelFactory
    {

        [TestMethod]
        public void testModelFactoryInsertUpdateMinimal()
        {
            var schemaFileName = "sys_dictionary_cmdb.json";
            var dataFileName = "cmdb_100.json";
            var tableName = "cmdb";
            runFullImportUpdate(schemaFileName, dataFileName, tableName, 100);
        }

        [TestMethod]
        public void testModelFactoryInsertUpdateCMDB_CI()
        {
            runFullImportUpdate("sys_dictionary_cmdb_ci_pp.json", "cmdb_ci_pp.json", "cmdb_ci", 10);
        }


        [TestMethod]
        public void testCleanupStaleTuples()
        {
            var schemaFileName = "sys_dictionary_cmdb.json";
            var dataFileName = "cmdb_100.json";
            var tableName = "cmdb";
            runFullImportUpdate(schemaFileName, dataFileName, tableName, 100);
            var lastSync = DateTime.Now;

            var nextDataFileName = "cmdb_5.json";

            var ctx = new SnowDbContext();
            var mig = new SnowMigration().Migrate(ctx, getDict(schemaFileName)); // getDictCMDB()

            using (var cnx = ctx.Database.GetDbConnection())
            {
                var tw = new TupleWriter() { Migration = mig };
                var tuples = getTestData(nextDataFileName);     //getTestCMDB();
                var updated = tw.UpdateTuples(cnx, tableName, tuples);
                var deleted = tw.CleanupStaleTuples(cnx, tableName, lastSync);
                Assert.AreEqual(95, deleted);
            }
        }

        public void runFullImportUpdate(string schemaFileName, string dataFileName, string tableName, int expectedTuples)
        {
            var ctx = InitDBSqlServer(true);
            var mig = new SnowMigration().Migrate(ctx, getDict(schemaFileName)); // getDictCMDB()
            var tuples = getTestData(dataFileName);     //getTestCMDB();
            Assert.AreEqual(expectedTuples, tuples.Count);

            using (var cnx = ctx.Database.GetDbConnection())
            {
                var inserted = new TupleWriter() { Migration = mig }.InsertTuples(cnx, tableName, tuples);
                Assert.AreEqual(tuples.Count, inserted);

                ValidateData(tableName, tuples.Count);

                var updated = new TupleWriter() { Migration = mig }.UpdateTuples(cnx, tableName, tuples);
                Assert.AreEqual(tuples.Count, updated);
            }
        }


        public void runGenerateCommandsFullImportUpdate(string schemaFileName, string dataFileName, string tableName, int expectedTuples)
        {
            var ctx = InitDBSqlServer(true);
            var migCommands = new SnowMigration().GenerateCommands(ctx, getDict(schemaFileName)); // getDictCMDB()
            Assert.IsNotNull(migCommands);
        }


        [TestMethod]
        public void testModelFactoryCreateCMDBMinimal()
        {
            var ctx = InitDBSqlServer(true);

            var mig = new SnowMigration().Migrate(ctx, getDictCMDB());

            var tuples = getTestCMDB();

            var tableName = "cmdb";
            var inserted = new TupleWriter() { Migration = mig }.InsertTuples(ctx, tableName, tuples);
            Assert.AreEqual(tuples.Count, inserted);

            ValidateData(tableName, tuples.Count);

            var updated = new TupleWriter() { Migration = mig }.UpdateTuples(ctx.New(), tableName, tuples);
            Assert.AreEqual(tuples.Count, inserted);

            var report = new TupleWriter() { Migration = mig }.WriteTuples(ctx.New(), tableName, tuples);
            Assert.AreEqual(tuples.Count, report.Written);
            Assert.AreEqual(tuples.Count, report.Updated);
        }

        [TestMethod]
        public override void testModelFactoryCreateCMDB()
        {
            //const string CONNECTION = "Data Source=SnowInMemory.db";
            var ctx = InitDBSqlServer(true);

            //var provider = ctx.GetDesignServices();
            //var databaseModelFactory = provider.GetRequiredService<IDatabaseModelFactory>();
            //var databaseModel = databaseModelFactory.Create(ctx.Database.GetDbConnection(), new DatabaseModelFactoryOptions());
            var databaseModel = ctx.CurrentModel;
            var mig = new SnowMigration();
            mig.Tables = getDictCMDB();
            mig.DbModel = databaseModel;

            //var migrationsSqlGenerator = ctx.Services.GetRequiredService<IMigrationsSqlGenerator>();
            //var ops = migrationsSqlGenerator.Generate(mig.UpOperations);
            var ops = mig.GenerateCommands(ctx);
            Xunit.Assert.NotEmpty(ops);
            Xunit.Assert.True(ops.All(o => o.CommandText.StartsWith("CREATE TABLE") || o.CommandText.StartsWith("CREATE INDEX")));

            //var cmdExec = ctx.Services.GetRequiredService<IMigrationCommandExecutor>();
            //cmdExec.ExecuteNonQuery(ops, ctx.Services.GetRequiredService<IRelationalConnection>());
            mig.Execute(ops, ctx);

            databaseModel = ctx.ModelFactory.Create(ctx.Database.GetDbConnection(), new DatabaseModelFactoryOptions());
            mig = new SnowMigration();
            mig.DbModel = databaseModel;
            var tuples = getTestCMDB();

            var tableName = "cmdb";
            var names = databaseModel.Tables.Where(t => t.Name == tableName).First().Columns.Select(c => c.Name).Join(",");
            var values = databaseModel.Tables.Where(t => t.Name == tableName).First().Columns.Select(c => "@" + c.Name).Join(",");

            using (var cnx = ctx.Database.GetDbConnection())
            {
                if (cnx.State != ConnectionState.Open) cnx.Open();
                try
                {
                    foreach (var tuple in tuples)
                    {
                        var sql = new StringBuilder("INSERT INTO [" + tableName + "] (");
                        sql.Append(names).Append(") values (");
                        sql.Append(values).Append(")");

                        var cmd = cnx.CreateCommand();
                        cmd.CommandText = sql.ToString();

                        var parms = new List<DbParameter>();
                        foreach (var col in databaseModel.Tables.Where(t => t.Name == tableName).First().Columns)
                        {
                            object tpVal = null;
                            try
                            {
                                var cp = cmd.CreateParameter();
                                cp.ParameterName = col.Name;
                                var token = tuple.SelectToken(col.Name);
                                if (token != null && token.HasValues && token.SelectToken("value") != null)
                                {
                                    tpVal = token.Value<string>("value");
                                }
                                else
                                {
                                    tpVal = tuple[col.Name];
                                }
                                cp.Value = SnowMigration.ToSqlType(col.StoreType, tpVal);
                                cp.DbType = SnowMigration.GetDbType(col.StoreType); // (DbType)Enum.Parse(typeof(DbType), c.StoreType);
                                parms.Add(cp);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("cannot map: " + col.Name + " = " + tpVal + " tuple=" + tuple, e);
                            }
                        }


                        cmd.Parameters.AddRange(parms.ToArray());
                        var res = cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    cnx.Close();
                }
            }

            ValidateData(tableName, 100);
        }


        [TestMethod]
        public void testModelFactoryCreateCmdbRelCiMinimal()
        {
            var ctx = InitDBSqlServer(true);

            var dict = getDict("sys_dictionary_cmdb_rel_ci.json");
            var mig = new SnowMigration().Migrate(ctx, dict);

            // added two indexes for SnowBase.SNOWDBSYNC_CREATED, SnowBase.SNOWDBSYNC_CREATED
            Assert.AreEqual(3+TupleWriter.SnowSyncColumns.Count, mig.Commands.Where(c => c.CommandText.StartsWith("CREATE INDEX")).Count());
        }

        protected void ValidateData(string tableName, int rowCount)
        {
            using (var cnx = new SnowDbContext().Database.GetDbConnection())
            {
                cnx.Open();
                var cmd = cnx.CreateCommand();
                cmd.CommandText = "select * from [" + tableName + "]";
                var res = cmd.ExecuteReader();
                Xunit.Assert.True(res.HasRows);
                var resList = new List<object>();
                while (res.Read()) resList.Add(res);
                Xunit.Assert.Equal(rowCount, resList.Count());
            }
        }

        public SnowDbContext InitDBSqlServer(bool delete = false)
        {
            dynamic entity = new SnowBase { sys_id_str = "abcd1234abcd1234abcd1234abcd1234" };
            entity.DynProp = "DynProp";

            //((object)entity).GetType().GetProperty("test").SetValue(entity, "test");
            var ctx = new SnowDbContext();
            ctx.DropAllTables();

            ctx = new SnowDbContext();
            ctx.Database.EnsureCreated();
            string idStr = entity.sys_id_str;
            var sbes = (from sb in ctx.SnowBases where sb.sys_id_str == idStr select sb).ToList();
            if (sbes.Count == 0)
            {
                ctx.SnowBases.Add(entity);
                ctx.SaveChanges();
            }
            return ctx;
        }
    }
}
