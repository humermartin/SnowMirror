using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Linq;
using MirrorRepository.REST;
using MirrorRepository;
using MirrorRepository.Base;
using System.Text;
using System.Data;
using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.Migrations.Design;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class TestModelFactory
    {
        /*
        [TestMethod]
        public void testSqlServerModelFactory()
        {

            IScaffoldingModelFactory _factory;
            //IOperationReporter rep = new OperationReporter();
            var services = new ServiceCollection()
                //.AddEntityFrameworkDesignTimeServices(_reporter)
                .AddSingleton<IScaffoldingModelFactory, SqlServerDatabaseModelFactory>();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

            _factory = services
                .BuildServiceProvider()
                .GetRequiredService<IScaffoldingModelFactory>();

        }
        */

        [TestMethod]
        public void testModelFactory()
        {
            const string CONNECTION = "Data Source=SnowInMemory.db";

            var ctx = InitDB(true);

            //var sqlitedmf = new SqliteDatabaseModelFactory();
            //var sqldmf = new SqlServerDatabaseModelFactory();
            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                //.AddSingleton<INamedConnectionStringResolver>(resolver)
                .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                //.AddSingleton<IRelationalTypeMappingSource>(RelationalTypeMappingSource.FindMapping()) // new RelationalTypeMappingSource >()
                //.AddSingleton<LoggingDefinitions>() //, TestRelationalLoggingDefinitions>()
                //.AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
                //.AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                ;

            var ctxServices = ((IInfrastructure<IServiceProvider>)ctx).Instance;

            services.AddDbContextDesignTimeServices(ctx);
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
            var provider = services.BuildServiceProvider();

            var databaseModelFactory = provider.GetRequiredService<IDatabaseModelFactory>();
            var databaseModel = databaseModelFactory.Create(CONNECTION, new DatabaseModelFactoryOptions()); // tables, schemas));

            var modelFactory = provider.GetRequiredService<IScaffoldingModelFactory>();
            var model = modelFactory.Create(databaseModel, true);
            model.GetEntityTypes();


            var scaffolder = provider.GetRequiredService<IReverseEngineerScaffolder>();

            var nsOpts = new ModelCodeGenerationOptions
            {
                ModelNamespace = "snow.db"
            };
            var result = scaffolder.ScaffoldModel(
                CONNECTION,
                new DatabaseModelFactoryOptions(),
                new ModelReverseEngineerOptions(),
                nsOpts);


            List<CompilerResults> crs = result.AdditionalFiles.Select(f => Compile(Path.GetFileNameWithoutExtension(f.Path), f.Code)).ToList();

            ModelBuilder builder = GetModelBuilder(ctx);
            crs.ForEach(cr => UpdateModel(builder, cr));
            IModel newModel = builder.Model;

            MigrationsScaffolder migScaffold = (MigrationsScaffolder)provider.GetService<IMigrationsScaffolder>();
            MigrationsScaffolderDependencies migDeps = (MigrationsScaffolderDependencies)
                typeof(MigrationsScaffolder).GetProperty("Dependencies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(migScaffold);
            IEnumerable<MigrationOperation> migOps = migDeps.MigrationsModelDiffer.GetDifferences(model, newModel);

            //Assert.Equal(CONNECTION, databaseModelFactory.ConnectionString);

            Xunit.Assert.Contains(CONNECTION, result.ContextFile.Code);
            //Xunit.Assert.DoesNotContain("Data Source=Test", result.ContextFile.Code);
            //Xunit.Assert.DoesNotContain("#warning", result.ContextFile.Code);

        }

        public void UpdateModel(ModelBuilder builder, CompilerResults cr)
        {
            EntityTypeBuilder ent = builder.Entity(cr.CompiledAssembly.DefinedTypes.Where(dt => dt.Name.Contains("Snow")).FirstOrDefault());
            ent.Property(typeof(string), "test");
        }


        [TestMethod]
        public void testModelFactoryCreate()
        {
            const string CONNECTION = "Data Source=SnowInMemory.db";
            var ctx = InitDB(true);

            var ctxServices = ((IInfrastructure<IServiceProvider>)ctx).Instance;
            //var services = new ServiceCollection().AddDbContextDesignTimeServices(ctx);
            //new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                ;
            services.AddDbContextDesignTimeServices(ctx);
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
            var provider = services.BuildServiceProvider();
            var databaseModelFactory = provider.GetRequiredService<IDatabaseModelFactory>();

            var databaseModel = databaseModelFactory.Create(CONNECTION, new DatabaseModelFactoryOptions());
            var mig = new SnowMigration();
            mig.Tables = getDict();
            mig.DbModel = databaseModel;
            //var migrator = ctxServices.GetRequiredService<IMigrator>();
            //migrator.GenerateScript(null, null, false);

            var cmdExec = ctxServices.GetRequiredService<IMigrationCommandExecutor>();
            var migrationsSqlGenerator = ctxServices.GetRequiredService<IMigrationsSqlGenerator>();
            var ops = migrationsSqlGenerator.Generate(mig.UpOperations);
            Xunit.Assert.NotEmpty(ops);
            Xunit.Assert.True(ops.Count() > 100);
            Xunit.Assert.True(ops.All(o => o.CommandText.StartsWith("CREATE TABLE") || o.CommandText.StartsWith("CREATE INDEX")));

            cmdExec.ExecuteNonQuery(ops, ctxServices.GetRequiredService<IRelationalConnection>());

            // reuse same model
            databaseModel = databaseModelFactory.Create(CONNECTION, new DatabaseModelFactoryOptions());
            var mig2 = new SnowMigration();
            mig2.Tables = getDict();
            mig2.DbModel = databaseModel;
            var ops2 = migrationsSqlGenerator.Generate(mig2.UpOperations);
            Xunit.Assert.Empty(ops2);

            // update model
            databaseModel = databaseModelFactory.Create(CONNECTION, new DatabaseModelFactoryOptions());
            var migUp = new SnowMigration();
            migUp.Tables = getDictUpdate();
            migUp.DbModel = databaseModel;
            var opsUp = migrationsSqlGenerator.Generate(migUp.UpOperations);
            Xunit.Assert.NotEmpty(opsUp);
            Xunit.Assert.Equal(2, opsUp.Count());
            Xunit.Assert.True(opsUp.All(o => o.CommandText.Contains("_UPDATE")));

            // update model new tables
            databaseModel = databaseModelFactory.Create(CONNECTION, new DatabaseModelFactoryOptions());
            var migUp2 = new SnowMigration();
            migUp2.Tables = getDictStartwithA();
            migUp2.DbModel = databaseModel;
            var opsUp2 = migrationsSqlGenerator.Generate(migUp2.UpOperations);
            Xunit.Assert.NotEmpty(opsUp2);
            Xunit.Assert.True(opsUp2.Count() > 100);
            Xunit.Assert.True(opsUp2.All(o => o.CommandText.StartsWith("CREATE TABLE") || o.CommandText.StartsWith("CREATE INDEX")));

        }


        [TestMethod]
        public virtual void testModelFactoryCreateCMDB()
        {
            //const string CONNECTION = "Data Source=SnowInMemory.db";
            var ctx = InitDB(true);

            var ctxServices = ((IInfrastructure<IServiceProvider>)ctx).Instance;

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                ;
            services.AddDbContextDesignTimeServices(ctx);
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
            var provider = services.BuildServiceProvider();
            var databaseModelFactory = provider.GetRequiredService<IDatabaseModelFactory>();

            var databaseModel = databaseModelFactory.Create(ctx.Database.GetDbConnection(), new DatabaseModelFactoryOptions());
            var mig = new SnowMigration();
            mig.Tables = getDictCMDB();
            mig.DbModel = databaseModel;

            var cmdExec = ctxServices.GetRequiredService<IMigrationCommandExecutor>();
            var migrationsSqlGenerator = ctxServices.GetRequiredService<IMigrationsSqlGenerator>();
            var ops = migrationsSqlGenerator.Generate(mig.UpOperations);
            Xunit.Assert.NotEmpty(ops);
            Xunit.Assert.True(ops.Count() >= 1);
            Xunit.Assert.True(ops.All(o => o.CommandText.StartsWith("CREATE TABLE") || o.CommandText.StartsWith("ALTER TABLE")
                || o.CommandText.StartsWith("CREATE INDEX")));

            cmdExec.ExecuteNonQuery(ops, ctxServices.GetRequiredService<IRelationalConnection>());

            databaseModel = databaseModelFactory.Create(ctx.Database.GetDbConnection(), new DatabaseModelFactoryOptions());
            mig = new SnowMigration();
            mig.DbModel = databaseModel;
            var tuples = getTestCMDB();

            var tableName = "cmdb";
            var names = databaseModel.Tables.Where(t => t.Name == tableName).First().Columns.Select(c => c.Name).Join(",");
            var values = databaseModel.Tables.Where(t => t.Name == tableName).First().Columns.Select(c => "@" + c.Name).Join(",");

            using (var cnx = ctx.Database.GetDbConnection())
            {
                cnx.Open();
                try {
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
                                } else
                                {
                                    tpVal = tuple.Value<object>(col.Name);
                                }
                                cp.Value = SnowMigration.ToSqlType(col.StoreType, tpVal, SnowMigration.SqlStorage.Sqllite);
                                cp.DbType = SnowMigration.GetDbType(col.StoreType, SnowMigration.SqlStorage.Sqllite); // (DbType)Enum.Parse(typeof(DbType), c.StoreType);
                                parms.Add(cp);
                            } catch (Exception e)
                            {
                                throw new Exception("cannot map: " + col.Name + " = " + tpVal + " tuple=" + tuple, e);
                            }
                        }


                        cmd.Parameters.AddRange(parms.ToArray());
                        var res = cmd.ExecuteNonQuery();
                    }
                } catch (Exception e)
                {
                    throw e;
                } finally
                {
                    cnx.Close();
                }
            } 

            using (var cnx = new SnowSyncInMemoryContext().SyncedContext.Database.GetDbConnection())
            {
                cnx.Open();
                var cmd = cnx.CreateCommand();
                cmd.CommandText = "select * from [" + tableName + "]";
                var res = cmd.ExecuteReader();
                Xunit.Assert.True(res.HasRows);
                var resList = new List<object>();
                while (res.Read()) resList.Add(res);
                Xunit.Assert.Equal(100, resList.Count());
            }

            {
                var sysIds = from c in ctx.SyncedContext.cmdb
                             where c.SysUpdatedOn < DateTime.Now
                             select c.SysId
                             .Take(100)
                             .ToList();
                Assert.IsNotNull(sysIds);
                Assert.IsTrue(sysIds.Count() > 0);
                Assert.AreEqual(sysIds.Count(), 100);
            }

        }


        [TestMethod]
        public void TestGetDict()
        {
            var dict = getDict();
        }

        [TestMethod]
        public void TestGetDictCMDB_CI()
        {
            var dict = getDict("sys_dictionary_cmdb_ci.json");
            Assert.IsTrue(dict.Values.Any(d => d.Any(e => e.element == "asset")));
        }

        [TestMethod]
        public void TestGetDictCMDB()
        {
            var dict = getDictCMDB();
            Assert.IsTrue(dict.Values.Any(d => d.Any(e => e.element == "asset")));
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> getDict(string jsonDictionaryFile = "dictionary_LIKEu_.json")
        {
            string content = File.ReadAllText(Path.Combine("testfiles", jsonDictionaryFile));
            var rc = new RestClient();
            return rc.ToTables(rc.Deserialize<DictionaryResponse>(content).result.ToList());
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> getDictUpdate(string jsonDictionaryFile = "dictionary_LIKEu_UPDATE.json")
        {
            string content = File.ReadAllText(Path.Combine("testfiles", jsonDictionaryFile));
            var rc = new RestClient();
            return rc.ToTables(rc.Deserialize<DictionaryResponse>(content).result.ToList());
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> getDictStartwithA()
        {
            string content = File.ReadAllText(Path.Combine("testfiles", "sys_dictionary_STARTSWITH_a.json"));
            var rc = new RestClient();
            return rc.ToTables(rc.Deserialize<DictionaryResponse>(content).result.ToList());
        }

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> getDictCMDB()
        {
            string content = File.ReadAllText(Path.Combine("testfiles", "sys_dictionary_cmdb.json"));
            var rc = new RestClient();
            return rc.ToTables(rc.Deserialize<DictionaryResponse>(content).result.ToList());
        }

        public List<JObject> getTestCMDB()
        {
            return getTestData("cmdb_100.json");
        }

        public List<JObject> getTestData(string jsonFileName)
        {
            string content = File.ReadAllText(Path.Combine("testfiles", jsonFileName));
            var rc = new RestClient();
            return rc.Deserialize<QueryResponse>(content).result.ToList();
        }

        public virtual SnowSyncInMemoryContext InitDB(bool delete = false)
        {
            if (delete)
            {
                if (File.Exists(SnowSyncInMemoryContext.INMEM_NAME))
                    File.Delete(SnowSyncInMemoryContext.INMEM_NAME);
            }

            dynamic entity = new SnowBase { sys_id_str = "abcd1234abcd1234abcd1234abcd1234" };
            entity.DynProp = "DynProp";

            //((object)entity).GetType().GetProperty("test").SetValue(entity, "test");
            var ctx = new SnowSyncInMemoryContext();
            string idStr = entity.sys_id_str;
            var sbes = (from sb in ctx.SnowBases where sb.sys_id_str == idStr select sb).ToList();
            if (sbes.Count == 0) {
                ctx.SnowBases.Add(entity);
                ctx.SaveChanges();
            }
            return ctx;
        }

        public static CompilerResults Compile(String name, String source, String package = "snow.db")
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerParameters cp = new CompilerParameters();

            // Generate an executable instead of a class library.
            cp.GenerateExecutable = false;
            // Specify the assembly file name to generate.
            cp.OutputAssembly = package;
            // Save the assembly as a physical file.
            cp.GenerateInMemory = true;
            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = false;
            // Invoke compilation of the source file.
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, source);

            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                Console.WriteLine("Errors building {0} into {1}",
                    source, cr.PathToAssembly);
                foreach (CompilerError ce in cr.Errors)
                {
                    Console.WriteLine("  {0}", ce.ToString());
                    Console.WriteLine();
                }
            }
            else
            {
                // Display a successful compilation message.
                Console.WriteLine("Source {0} built into {1} successfully.",
                    name, cr.PathToAssembly);
            }

            // Return the results of the compilation.
            if (cr.Errors.Count > 0)
            {
                return null;
            }
            else
            {
                return cr;
            }
            //return compileOk;
        }

        private ModelBuilder GetModelBuilder(Microsoft.EntityFrameworkCore.DbContext ctx, Microsoft.EntityFrameworkCore.DbContext dbContext = null)
        {
            var conventionSet = new ConventionSet();

            var dependencies = CreateDependencies(ctx)
                .With(new CurrentDbContext(dbContext ?? 
                new Microsoft.EntityFrameworkCore.DbContext(new DbContextOptions<Microsoft.EntityFrameworkCore.DbContext>())));
            var relationalDependencies = CreateRelationalDependencies(ctx);
            var dbFunctionAttributeConvention = new RelationalDbFunctionAttributeConvention(dependencies, relationalDependencies);
            conventionSet.ModelInitializedConventions.Add(dbFunctionAttributeConvention);
            conventionSet.ModelAnnotationChangedConventions.Add(dbFunctionAttributeConvention);
            conventionSet.ModelFinalizedConventions.Add(new DbFunctionTypeMappingConvention(dependencies, relationalDependencies));

            return new ModelBuilder(conventionSet);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies(Microsoft.EntityFrameworkCore.DbContext ctx)
            => ((IInfrastructure<IServiceProvider>)ctx).Instance.GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private RelationalConventionSetBuilderDependencies CreateRelationalDependencies(Microsoft.EntityFrameworkCore.DbContext ctx)
            => ((IInfrastructure<IServiceProvider>)ctx).Instance.GetRequiredService<RelationalConventionSetBuilderDependencies>();
    }

    /*
     private IMigrationsScaffolder CreateMigrationScaffolder<TContext>()
     where TContext : DbContext, new()
         {
             var currentContext = new CurrentDbContext(new TContext());
             var idGenerator = new MigrationsIdGenerator();
             var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
                 TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                 TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());
             var code = new CSharpHelper(sqlServerTypeMappingSource);
             var reporter = new TestOperationReporter();
             var migrationAssembly
                 = new MigrationsAssembly(
                     currentContext,
                     new DbContextOptions<TContext>().WithExtension(new FakeRelationalOptionsExtension()),
                     idGenerator,
                     new FakeDiagnosticsLogger<DbLoggerCategory.Migrations>());
             var historyRepository = new MockHistoryRepository();

             var services = RelationalTestHelpers.Instance.CreateContextServices();
             IModel model = new Model();
             model = new RelationalModelConvention().ProcessModelFinalized(model);

             return new MigrationsScaffolder(
                 new MigrationsScaffolderDependencies(
                     currentContext,
                     model,
                     migrationAssembly,
                     new MigrationsModelDiffer(
                         new TestRelationalTypeMappingSource(
                             TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                             TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                         new MigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies()),
                         services.GetRequiredService<IChangeDetector>(),
                         services.GetRequiredService<IUpdateAdapterFactory>(),
                         services.GetRequiredService<CommandBatchPreparerDependencies>()),
                     idGenerator,
                     new MigrationsCodeGeneratorSelector(
                         new[]
                         {
                             new CSharpMigrationsGenerator(
                                 new MigrationsCodeGeneratorDependencies(sqlServerTypeMappingSource),
                                 new CSharpMigrationsGeneratorDependencies(
                                     code,
                                     new CSharpMigrationOperationGenerator(
                                         new CSharpMigrationOperationGeneratorDependencies(
                                             code)),
                                     new CSharpSnapshotGenerator(
                                         new CSharpSnapshotGeneratorDependencies(
                                             code, sqlServerTypeMappingSource))))
                         }),
                     historyRepository,
                     reporter,
                     new MockProvider(),
                     new SnapshotModelProcessor(reporter),
                     new Migrator(
                         migrationAssembly,
                         historyRepository,
                         services.GetRequiredService<IDatabaseCreator>(),
                         services.GetRequiredService<IMigrationsSqlGenerator>(),
                         services.GetRequiredService<IRawSqlCommandBuilder>(),
                         services.GetRequiredService<IMigrationCommandExecutor>(),
                         services.GetRequiredService<IRelationalConnection>(),
                         services.GetRequiredService<ISqlGenerationHelper>(),
                         services.GetRequiredService<ICurrentDbContext>(),
                         services.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Migrations>>(),
                         services.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>(),
                         services.GetRequiredService<IDatabaseProvider>())));
         } */
}
