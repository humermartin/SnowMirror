using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using MirrorRepository.Base;
using MirrorRepository.Model.Snow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MirrorRepository 
{

    public class SnowDbModelSnapshot : ModelSnapshot
    {
        public Dictionary<SnowDictEntry, List<SnowDictEntry>> Tables { get; set; }
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            var eb = modelBuilder.Entity("snowdb.");
            eb.Property<int>("Id").ValueGeneratedOnAdd();
            eb.Property<string>("Name");
            eb.HasKey("Id");
            eb.ToTable("Blogs");
        }
    }
    public class SnowMigration : Migration
    {
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public string SyncName { get; set; } = "default";
        public string SnowTablePrefix { get; set; } = "";

        public Dictionary<SnowDictEntry, List<SnowDictEntry>> Tables { get; set; } = new Dictionary<SnowDictEntry, List<SnowDictEntry>>();
        public DatabaseModel DbModel { get; set; }

        /// <summary>
        /// Type of systemwide sys_id
        /// must be same as the referencing foreign keys! 
        /// </summary>
        public static Type SysIdType { get; set; } = typeof(string); //alternative: typeof(Guid);
        public IReadOnlyList<MigrationCommand> Commands { get; private set; } = new List<MigrationCommand>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SnowMigration Init(SnowDbContext ctx)
        {
            if (DbModel == null) DbModel = ctx.CurrentModel;
            return this;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Up(MigrationBuilder mb)
        {
            CreateModel(mb, Tables);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Down(MigrationBuilder mb)
        {
            //base.Down();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SnowMigration Migrate(SnowDbContext ctx, Dictionary<SnowDictEntry, List<SnowDictEntry>> tables = null)
        {
            try
            {
                if (tables != null) Tables = tables;
                if (DbModel == null) DbModel = ctx.CurrentModel;
                Commands = GenerateCommands(ctx);
                Execute(Commands, ctx);
                DbModel = ctx.CurrentModel;
                if (Tables != null)
                {
                    Log.Debug("migrating["+SyncName+"] " + string.Join(",", Tables.Keys.Select(k => k.name)) + ", ctx=" + ctx);
                } else
                {
                    Log.Debug("migrating["+SyncName+"] null tables, ctx=" + ctx);
                }
                if (Commands != null && Commands.Count > 0)
                {
                    foreach (var cmd in Commands)
                    {
                        Log.Info("executed["+SyncName+"]: " + cmd.CommandText);
                    }
                } else
                {
                    Log.Info("migration[["+SyncName+"]: no comands to execute for: " + Tables?.Keys.Select(k=>k.name));
                }
                if (Tables != null)
                {
                    Log.Info("migrated["+SyncName+"]: " + string.Join(",", Tables.Keys.Select(k => k.name)) + ", ctx=" + ctx);
                }
                else
                {
                    Log.Info("migrated["+SyncName+"]: null tables, ctx=" + ctx);
                }
                return this;
            } catch (Exception e)
            {
                Log.Info("cannot migrate " + string.Join(",", tables.Keys.Select(k => k.name)) + ", ctx=" + ctx, e);
                foreach (var cmd in Commands)
                {
                    Log.Info("commands: " + cmd);
                }
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public SnowMigration GenerateCommands(SnowDbContext ctx, Dictionary<SnowDictEntry, List<SnowDictEntry>> tables = null)
        {
            if (tables != null) Tables = tables;
            if (DbModel == null) DbModel = ctx.CurrentModel;
            Commands = GenerateCommands(ctx);
            return this;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IReadOnlyList<MigrationCommand> GenerateCommands(SnowDbContext ctx)
        {
            var migrationsSqlGenerator = ctx.Services.GetRequiredService<IMigrationsSqlGenerator>();
            var ops = migrationsSqlGenerator.Generate(UpOperations);
            return ops;
        }

        public void Execute(IReadOnlyList<MigrationCommand> commands, SnowDbContext ctx)
        {
            var cmdExec = ctx.Services.GetRequiredService<IMigrationCommandExecutor>();
            cmdExec.ExecuteNonQuery(commands, ctx.Services.GetRequiredService<IRelationalConnection>());
        }

        public void CreateModel(MigrationBuilder mb, Dictionary<SnowDictEntry, List<SnowDictEntry>> dict)
        {
            foreach (var table in dict.Keys)
            {
                var props = dict[table];
                if (!DbModel.Tables.Any(t => t.Name == GetTableName(table)))
                {
                    var createTable = new CreateTableOperation() { Name = GetTableName(table) };
                    createTable.Columns.AddRange(CreateColumns(table, props));
                    //var tableBuilder = mb.CreateTable(table.name, cb => CreateColumns(table, props));
                    createTable.PrimaryKey = new AddPrimaryKeyOperation { 
                        Table = GetTableName(table), 
                        Name = GetTablePKName(table), 
                        Columns = new string[] { SnowBase.SYS_ID } };
                    mb.Operations.Add(createTable);
                    foreach (var index in GetTableIndexNames(table, props))
                    {
                        mb.CreateIndex(index.Key, GetTableName(table), index.Value);
                    }
                }
                else
                {
                    foreach (var prop in props)
                    {
                        var modelTable = DbModel.Tables.FirstOrDefault(t => t.Name == GetTableName(table));
                        if (!modelTable.Columns.Any(c => c.Name == prop.element))
                        {
                            mb.Operations.Add(AddNewColumn(table, prop));
                        }
                    }
                }
                
                //var tableBuilder = new TableBuilder<SnowBase>(createTable, this);
                //ent.HasBaseType(typeof(SnowBase));
                //ent.Property(SysIdType, "sys_id");
            }
        }

        public string GetTableName(SnowDictEntry table)
        {
            return GetTableName(table.name);
        }

        public string GetTableName(string tableName)
        {
            return SnowTablePrefix + tableName;
        }

        public string GetTablePKName(SnowDictEntry table)
        {
            return GetTablePKName(table.name);
        }

        public string GetTablePKName(string tableName)
        {
            return GetTableName(tableName) + "_" + SnowBase.SYS_ID + "_pk";
        }

        public Dictionary<string, string> GetTableIndexNames(SnowDictEntry table, List<SnowDictEntry> columns)
        {
            return GetTableIndexNames(table.name, columns.Where(c => c.IsReference).Select(c => c.element).ToList());
        }


        public Dictionary<string, string> GetTableIndexNames(string tableName, List<string> columnNames )
        {
            var dict = columnNames.ToDictionary(c => GetTableName(tableName) + "_" + c + "_idx", c => c);
            dict.Add(GetTableName(tableName) + "_" + SnowBase.SNOWDBSYNC_CREATED + "_idx", SnowBase.SNOWDBSYNC_CREATED);
            dict.Add(GetTableName(tableName) + "_" + SnowBase.SNOWDBSYNC_UPDATED + "_idx", SnowBase.SNOWDBSYNC_UPDATED);
            dict.Add(GetTableName(tableName) + "_" + SnowBase.KAFKA_SYNCHRONIZED + "_idx", SnowBase.KAFKA_SYNCHRONIZED);
            return dict;
        }

        public ColumnOperation AddNewColumn(SnowDictEntry table, SnowDictEntry column)
        {
            return GetColumnModel(table, column);
        }

        public List<AddColumnOperation> CreateColumns(SnowDictEntry table, List<SnowDictEntry> columns)
        {
            var newCols = new List<AddColumnOperation>();

            var sysId = columns.Where(e => e.element == SnowBase.SYS_ID).FirstOrDefault();
            if (sysId != null) columns.Remove(sysId);
            newCols.Add(new AddColumnOperation() { 
                ClrType = SysIdType, 
                MaxLength = 32,
                Table = GetTableName(table), 
                Name = SnowBase.SYS_ID, 
                IsNullable = false });

            foreach (var column in columns)
            {
                //ent.Property(GetType(prop), prop.element);
                var col = GetColumnModel(table, column);
                newCols.Add(col);
            }

            newCols.Add(new AddColumnOperation() { ClrType = typeof(DateTime), Table = GetTableName(table), Name = SnowBase.SNOWDBSYNC_CREATED, IsNullable = true });
            newCols.Add(new AddColumnOperation() { ClrType = typeof(DateTime), Table = GetTableName(table), Name = SnowBase.SNOWDBSYNC_UPDATED, IsNullable = true });
            newCols.Add(new AddColumnOperation() { ClrType = typeof(DateTime), Table = GetTableName(table), Name = SnowBase.KAFKA_SYNCHRONIZED, IsNullable = true });
            return newCols;
        }

        public AddColumnOperation GetColumnModel(SnowDictEntry table, SnowDictEntry column)
        {
            AddColumnOperation model;
            var tableName = GetTableName(table);
            var cType = GetType(column);
            switch (cType.Name)
            {
                // anything to do??
                default:
                    model = new AddColumnOperation() { ClrType = cType, Table = tableName, Name = column.element, IsNullable = true };
                    if (column.IsReference) model.MaxLength = 32;
                    break;
            }
            return model;
        }

        //public void CreateColumns(CreateTableOperation createTable, SnowDictEntry table, List<SnowDictEntry> columns)
        //{
        //    var sysId = columns.Where(e => e.name == "sys_id").FirstOrDefault();
        //    if (sysId != null) columns.Remove(sysId);

        //    createTable.Columns.Add(new ColumnModel(PrimitiveTypeKind.Guid) { Name = "sys_id", IsNullable = false });
        //    createTable.PrimaryKey = new AddPrimaryKeyOperation() { Name = "sys_id" };

        //    foreach (var prop in columns)
        //    {
        //        //ent.Property(GetType(prop), prop.name);
        //        createTable.Columns.Add(GetColumnModel(prop));
        //    }
        //}

        /*
        public AddColumnOperation GetColumnModel(SnowDictEntry table, SnowDictEntry column)
        {
            AddColumnOperation model;
            var tableName = GetTableName(table);
            switch (GetType(column).Name)
            {
                case "Binary":
                    model = new AddColumnOperation() { ClrType = typeof(byte[]), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Boolean":
                    model = new AddColumnOperation() { ClrType = typeof(Boolean), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Byte":
                    model = new AddColumnOperation() { ClrType = typeof(Byte), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "DateTime":
                    model = new AddColumnOperation() { ClrType = typeof(DateTime), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Decimal":
                    model = new AddColumnOperation() { ClrType = typeof(Decimal), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Double":
                    model = new AddColumnOperation() { ClrType = typeof(Double), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Int":
                    model = new AddColumnOperation() { ClrType = typeof(Int32), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Long":
                    model = new AddColumnOperation() { ClrType = typeof(Int64), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Short":
                    model = new AddColumnOperation() { ClrType = typeof(Int16), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                case "Time":
                    model = new AddColumnOperation() { ClrType = typeof(DateTime), Table = tableName, Name = column.element, IsNullable = true };
                    break;
                default:
                    model = new AddColumnOperation() { ClrType = typeof(String), Table = tableName, Name = column.element, IsNullable = true };
                    break;
            }
            return model;
        }
        */


        public enum SqlStorage { SqlServer, Sqllite }

        public static DbType GetDbType(string storageType, SqlStorage storage = SqlStorage.SqlServer)
        {
            DbType dt;
            switch (storage)
            {
                case SqlStorage.Sqllite:
                    switch (storageType)
                    {
                        case "NULL": dt = DbType.String; break;
                        case "INTEGER": dt = DbType.Int64; break;
                        case "REAL": dt = DbType.Double; break;
                        case "TEXT": dt = DbType.String; break;
                        case "BLOB": dt = DbType.Binary; break;
                        default:
                            dt = DbType.String;
                            break;
                    }
                    break;
                default:
                    switch (storageType)
                    {
                        case "bigint": dt = DbType.Int64; break;
                        case "int": dt = DbType.Int64; break;
                        case "smallint": dt = DbType.Int16; break;
                        case "tinyint": dt = DbType.Int16; break;
                        case "bit": dt = DbType.Int16; break;

                        case "decimal": dt = DbType.Decimal; break;
                        case "numeric": dt = DbType.Double; break;
                        case "smallmoney": dt = DbType.Decimal; break;
                        case "float": dt = DbType.Decimal; break;
                        case "real": dt = DbType.Decimal; break;

                        case "date": dt = DbType.Date; break;
                        case "datetimeoffset": dt = DbType.DateTime2; break;
                        case "datetime2": dt = DbType.DateTime2; break;
                        case "smalldatetime": dt = DbType.DateTime2; break;
                        case "datetime": dt = DbType.DateTime2; break;
                        case "time": dt = DbType.Time; break;

                        case "char": dt = DbType.String; break;
                        case "varchar": dt = DbType.String; break;
                        case "text": dt = DbType.String; break;
                        case "nchar": dt = DbType.String; break;
                        case "nvarchar": dt = DbType.String; break;
                        case "ntext": dt = DbType.String; break;

                        case "binary": dt = DbType.Binary; break;
                        case "varbinary": dt = DbType.Binary; break;
                        case "image": dt = DbType.Binary; break;
                        case "rowversion": dt = DbType.Int64; break;
                        case "hierarchyid": dt = DbType.Int64; break;
                        case "uniqueidentifier": dt = DbType.Guid; break;
                        default:
                            dt = DbType.String;
                            break;
                    }
                    break;
            }
            return dt;
        }

        public static object ToSqlType(string storageType, object value, SqlStorage storage = SqlStorage.SqlServer)
        {
            if (value is JValue) value = ((JValue)value).Value;
            if (value == null || (value is string && String.IsNullOrEmpty((string)value))) 
                return DBNull.Value;
            if (value is DateTime)
                return value;

            object colVal;
            bool boolVal;
            if (Boolean.TryParse(Convert.ToString(value), out boolVal)) value = boolVal;
 
            switch (storage)
            {
                case SqlStorage.Sqllite:
                    switch (storageType)
                    {
                        case "NULL": colVal = null; break;
                        case "INTEGER": colVal = value is bool ? colVal = ((bool)value ? 1 : 0) : Convert.ToInt64(value); break;
                        case "REAL": colVal = Convert.ToDouble(value); break;
                        case "TEXT": colVal = value; break;
                        case "BLOB": colVal = Encoding.UTF8.GetBytes(""+value); break;
                        default:
                            colVal = "" + value;
                            break;
                    }
                    break;
                default:
                    switch (storageType)
                    {
                        case "bigint": colVal = value is bool ? ((bool)value ? 1 : 0) : Convert.ToInt64(value); break;
                        case "int": colVal = value is bool ? ((bool)value ? 1 : 0) : Convert.ToInt64(value); break;
                        case "smallint": colVal = value is bool ? ((bool)value ? 1 : 0) : Convert.ToInt16(value); break;
                        case "tinyint": colVal = value is bool ? ((bool)value ? 1 : 0) : Convert.ToInt16(value); break;
                        case "bit": colVal = Convert.ToBoolean(value); break;

                        case "decimal": colVal = Convert.ToDecimal(value); break;
                        case "numeric": colVal = Convert.ToDouble(value); break;
                        case "smallmoney": colVal = Convert.ToDecimal(value); break;
                        case "float": colVal = Convert.ToDecimal(value); break;
                        case "real": colVal = Convert.ToDecimal(value); break;

                        case "date": colVal = SnowBase.ToDate(Convert.ToString(value)); break;
                        case "datetimeoffset": colVal = SnowBase.ToDate(Convert.ToString(value)); break;
                        case "datetime2": colVal = SnowBase.ToDate(Convert.ToString(value)); break;
                        case "smalldatetime": colVal = SnowBase.ToDate(Convert.ToString(value)); break;
                        case "datetime": colVal = SnowBase.ToDate(Convert.ToString(value)); break;
                        case "time": colVal = SnowBase.ToDate(Convert.ToString(value)); break;

                        case "char": colVal = value; break;
                        case "varchar": colVal = value; break;
                        case "text": colVal = value; break;
                        case "nchar": colVal = value; break;
                        case "nvarchar": colVal = value; break;
                        case "ntext": colVal = value; break;

                        case "binary": colVal = value is object[] ? value : Encoding.UTF8.GetBytes(Convert.ToString(value)); break;
                        case "varbinary": colVal = value is object[]? value : Encoding.UTF8.GetBytes(Convert.ToString(value)); break;
                        case "image": colVal = value is object[]? value : Encoding.UTF8.GetBytes(Convert.ToString(value)); break;
                        case "rowversion": colVal = Convert.ToInt64(value); break;
                        case "hierarchyid": colVal = Convert.ToInt64(value); break;
                        case "uniqueidentifier": colVal = Guid.Parse(Convert.ToString(value)); break;
                        default:
                            colVal = "" + value;
                            break;
                    }
                    break;
            }
            return colVal;
        }
        public Type GetType(SnowDictEntry entry)
        {
            switch (entry.element)
            {
                case "sys_id": // see: SnowBase.SYS_ID 
                    return SysIdType; 
                case "sys_created_on": 
                    return typeof(DateTime); 
                case "sys_updated_on": 
                    return typeof(DateTime); 
                case "sys_mod_count":
                    return typeof(Int64);
                case "sys_package":
                    return SysIdType;
                default:
                    var rex = new Regex(@".*=(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    try
                    {
                        var match = rex.Match(entry.internal_type.link);
                        if (match.Success)
                        {
                            var type = match.Groups[1];
                            if (SnowTypes.ContainsKey(type.Value))
                            {
                                return SnowTypes[type.Value];
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                    return typeof(string);
            }
        }

        public static readonly Dictionary<string, Type> SnowTypes = new Dictionary<string, Type> {
            { "auto_increment", typeof(string) },
            { "boolean", typeof(bool?) },
            { "breakdown_element", typeof(string) },
            { "char", typeof(char) },
            { "choice", typeof(string) },
            { "collection", typeof(string) },
            { "color", typeof(string) },
            { "compressed", typeof(string) },
            { "condition", typeof(string) },
            { "conditions", typeof(string) },
            { "counter", typeof(string) },
            { "currency", typeof(string) },
            { "data_array", typeof(string) },
            { "data_object", typeof(string) },
            { "date_time", typeof(DateTime) },
            { "datetime", typeof(DateTime) },
            { "decimal", typeof(float?) },
            { "document_id", typeof(string) },
            { "domain_id", typeof(string) },
            { "domain_path", typeof(string) },
            { "double", typeof(double?) },
            { "field_list", typeof(string) },
            { "field_name", typeof(string) },
            { "float", typeof(float?) },
            { "glide_action_list", typeof(string) },
            { "glide_date", typeof(DateTime) },
            { "glide_date_time", typeof(DateTime) },
            { "glide_duration", typeof(DateTime) },
            { "glide_list", typeof(string) },
            { "glide_time", typeof(DateTime) },
            { "glide_var", typeof(string) },
            { "GUID", SysIdType },
            { "html", typeof(string) },
            { "icon", typeof(string) },
            { "integer", typeof(long?) },
            { "integer_date", typeof(long?) },
            { "internal_type", typeof(string) },
            { "ip_address", typeof(string) },
            { "journal", typeof(string) },
            { "json", typeof(string) },
            { "longint", typeof(long?) },
            { "multi_small", typeof(string) },
            { "multi_two_lines", typeof(string) },
            { "name_values", typeof(string) },
            { "password", typeof(string) },
            { "password2", typeof(string) },
            { "percent_complete", typeof(string) },
            { "price", typeof(string) },
            { "records", typeof(string) },
            { "reference", typeof(string) }, // typeof(Guid) not working - TFR see:          
                /* CMDB: "vendor" : { "link" : "https://a1int.service-now.com/api/now/table/core_company/VMware", "value" : "VMware" },*/
            { "replication_payload", typeof(string) },
            { "script", typeof(string) },
            { "script_plain", typeof(string) },
            { "short_table_name", typeof(string) },
            { "simple_name_values", typeof(string) },
            { "slushbucket", typeof(string) },
            { "string", typeof(string) },
            { "string_full_utf8", typeof(string) },
            { "sys_class_name", typeof(string) },
            { "sys_class_path", typeof(string) },
            { "sysevent_name", typeof(string) },
            { "table_name", typeof(string) },
            { "template_value", typeof(string) },
            { "translated_field", typeof(string) },
            { "translated_html", typeof(string) },
            { "translated_text", typeof(string) },
            { "url", typeof(string) },
            { "user_image", typeof(string) },
            { "user_roles", typeof(string) },
            { "workflow", typeof(string) },
            { "xml", typeof(string) },
        };

        public override string ToString()
        {
            return "Migration[" + SyncName + "] pfx=" + SnowTablePrefix + ", " +
                "Tables=" + String.Join(",", DbModel?.Tables?.Select(t => "" + t.Name + "," + t.Schema + "," + t.Database).ToList());
        }
    }
}
