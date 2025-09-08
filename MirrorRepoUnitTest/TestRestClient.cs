using System;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository.REST;
using MirrorRepository.Base;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace MirrorRepoUnitTest
{

    [TestClass]
    public class TestRestClient
    {
        public static readonly string USER = "servicenow.srv.snowdbsync";
        public static readonly string PWD = "].8fgD>W";
        public static readonly string HOST = "a1int.service-now.com";
        public static readonly string PORT = "443";
        public static readonly string PROXYHOST = "proxy.austria.local";
        public static readonly int PROXYPORT = 8080;
        public static readonly string PROXYUSER = "dk_BPS-EGB2B_Develop";
        public static readonly string PROXYPASS = "skMKTGpJpsd^60*@Nmi1Kg^HL$";
        public static readonly string BaseUrl = "https://"+HOST+":"+PORT;

        [TestMethod] 
        public void TestFormatQueryWithColumns()
        {
            var columns = new List<string> { "test1", "GAGA3" };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var url = rc.FormatRequest("/api/now/table/cmdb", SnowParms.New.orderBy(PROP.sys_created_on).limit(5).columns(columns));
            Assert.IsTrue(url.Contains(string.Join("%2C", columns)));
            Assert.IsTrue(url.Contains(Enum.GetName(typeof(PARM), PARM.sysparm_fields)+"="+string.Join("%2C", columns)));

            url = rc.FormatRequest("/api/now/table/cmdb", SnowParms.New.orderBy(PROP.sys_created_on).limit(5).columns(new List<string> { }));
            Assert.IsFalse(url.Contains(Enum.GetName(typeof(PARM), PARM.sysparm_fields)));
        }

        [TestMethod]
        public void TestFormatQueryInList()
        {
            var columns = new List<string> { "test1", "GAGA3" };
            var sysids = new List<string> { "sys_id_1", "sys_id_2", "sys_id_3", "sys_id_4" };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var url = rc.FormatRequest("/api/now/table/cmdb", SnowParms.New.inList(PROP.sys_id, sysids).orderBy(PROP.sys_created_on).limit(5).columns(columns));
            Assert.IsTrue(url.Contains(string.Join("%2C", columns)));
            Assert.IsTrue(url.Contains(Enum.GetName(typeof(PARM), PARM.sysparm_fields) + "=" + string.Join("%2C", columns)));

            url = rc.FormatRequest("/api/now/table/cmdb", SnowParms.New.orderBy(PROP.sys_created_on).limit(5).columns(new List<string> { }));
            Assert.IsFalse(url.Contains(Enum.GetName(typeof(PARM), PARM.sysparm_fields)));
            Assert.IsFalse(url.Contains("sys_id=sys_id_1^ORsys_id=sys_id_2^ORsys_id=sys_id_3^ORsys_id=sys_id_4"));
        }

        [TestMethod]
        public void TestReadJsonData()
        {
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/cmdb", SnowParms.New.orderBy(PROP.sys_created_on).limit(5));
            var result = rc.Deserialize<QueryResponse>(content);
            Assert.AreEqual(5, result.result.Length);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(result.result[i].Value<DateTime>("sys_created_on") < result.result[i + 1].Value<DateTime>("sys_created_on"));
            }
        }

        [TestMethod]
        public void TestReadJsonDataOrderByMult()
        {
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/cmdb", 
                SnowParms.New
                .orderBy(PROP.sys_created_on)
                .orderBy(PROP.sys_id)
                .limit(5));
            var result = rc.Deserialize<QueryResponse>(content);
            Assert.AreEqual(5, result.result.Length);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(result.result[i].Value<DateTime>("sys_created_on") < result.result[i + 1].Value<DateTime>("sys_created_on"));
            }
        }

        [TestMethod]
        public void TestReadJsonSchema()
        {
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/sys_db_object", SnowParm.select(new string[] { "name", "sys_id" }));
            var dict = rc.Deserialize<DictionaryResponse>(content);
        }

        [TestMethod]
        public void TestReadJsonSchemaUserDefined()
        {
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/sys_db_object",
                SnowParm.select(new string[] { "name", "sys_id","label","sys_name" }).filter("name", OP.STARTSWITH, "u_"));
            var schema = rc.Deserialize<SchemaResponse>(content);
            Assert.IsTrue(schema.result.All(t => t.name.StartsWith("u_")));
        }

        [TestMethod]
        public void TestReadJsonDictionary()
        {
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/sys_dictionary", SnowParm.select(new string[] { "name", "sys_id" }));
            var dict = rc.Deserialize<DictionaryResponse>(content);
        }

        [TestMethod]
        public void TestReadJsonDictionaryLimit()
        {
            JsonSerializer ser = new JsonSerializer
            {
                DateFormatString = SnowBase.SNOW_DATETIME_FORMAT
            };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/sys_dictionary", SnowParm
                .select(new string[] { "sys_name","name","sys_id","active" })
                .active()
                .orderBy(PROP.sys_id).limit(10));
            var schema = rc.Deserialize<DictionaryResponse>(content);
            Assert.AreEqual(10, schema.result.Length);

            content = rc.Read("/api/now/table/sys_dictionary", SnowParm
                .select(new string[] { "sys_name","name","sys_id","active" })
                .active()
                .orderBy(PROP.sys_id).limit(10).offset(10));
            var schema2 = rc.Deserialize<DictionaryResponse>(content);
            var join = schema.result.Select(a => schema2.result.Where(b => b.sys_id_str == a.sys_id_str)).ToList();
            Assert.IsFalse(join.Where(j => j.Count() > 0).Count() > 0);
        }

        [TestMethod]
        public void TestReadJsonDictionaryUserDefined()
        {
            JsonSerializer ser = new JsonSerializer
            {
                DateFormatString = SnowBase.SNOW_DATETIME_FORMAT
            };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.Read("/api/now/table/sys_dictionary", 
                SnowParm.select(new string[] { "name", "sys_id" }).filter("name", OP.STARTSWITH, "u_"));
            var schema = rc.Deserialize<DictionaryResponse>(content);
            Assert.IsTrue(schema.result.All(t => t.name.StartsWith("u_")));
        }

        [TestMethod]
        public void TestReadJsonDictionaryUserDefinedTableAndFields()
        {
            JsonSerializer ser = new JsonSerializer
            {
                DateFormatString = SnowBase.SNOW_DATETIME_FORMAT
            };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            // read tables
            var content = rc.Read("/api/now/table/sys_dictionary",
                SnowParm.select(new string[] { "name", "sys_id", "element", "sys_name" })
                .empty("element")
                .orderByDesc("name"));// .notLike("name", "u_")) ; Not working???
            var snowTables = rc.Deserialize<DictionaryResponse>(content);
            var table = snowTables.result.Where(t => !t.name.StartsWith("u_")).First(); 
            // read Table entry
            content = rc.Read("/api/now/table/sys_dictionary",
                SnowParm.select(new string[] { "name", "sys_id","element","sys_name" })
                .empty("element")
                .equals("name", table.name));
            var tables = rc.Deserialize<DictionaryResponse>(content);
            // read fields of table entry
            content = rc.Read("/api/now/table/sys_dictionary",
                SnowParm.select(new string[] { "name", "sys_id", "element", "sys_name" })
                .notEmpty("element")
                .equals("name", table.name));
            var fields = rc.Deserialize<DictionaryResponse>(content);
            Assert.IsTrue(tables.result.Length == 1);
            Assert.IsTrue(tables.result.All(t => t.element == ""));
            Assert.IsTrue(fields.result.All(t => t.element != ""));
            Assert.IsTrue(tables.result.All(t => t.name.StartsWith(table.name)));
            Assert.IsTrue(fields.result.All(t => t.name.StartsWith(table.name)));
        }

        [TestMethod]
        public void TestReadDictionary()
        {
            JsonSerializer ser = new JsonSerializer
            {
                DateFormatString = SnowBase.SNOW_DATETIME_FORMAT
            };
            var rc = RestClient.Build(BaseUrl, USER, PWD);
            var content = rc.ReadDictionary("name", "u_");
            Assert.IsTrue(content.Keys.Count() > 0);
            foreach (var key in content.Keys) 
                Assert.IsTrue(content[key].Count() > 0);
        }

        [TestMethod]
        public void TestParseJsonSchema()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) { HandleError(sender, args); }
            };
            JsonSerializer ser = JsonSerializer.Create(jsonSettings);
            ser.DateFormatString = SnowBase.SNOW_DATETIME_FORMAT;

            var content = File.ReadAllText(Path.Combine("testfiles","sys_dictionary.json"));
            var schema = ser.Deserialize<SchemaResponse>(new JsonTextReader(new StringReader(content)));

            //Assert.IsFalse(schema.result.Any(t => t == null));
            Assert.IsFalse(schema.result.Any(t => t != null && t.name == null));
        }

        public void HandleError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            string line = "cannot parse: " + sender + " : " + args.ErrorContext.Error + " : " + args.ErrorContext.OriginalObject + " : " + args.ErrorContext.Path;
            Console.WriteLine(line);
        }

    }
}
