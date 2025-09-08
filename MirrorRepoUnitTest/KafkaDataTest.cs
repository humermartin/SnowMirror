using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository.Model.Kafka;
using Newtonsoft.Json;
using System.IO;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class KafkaDataTest
    {
        [TestMethod]
        public void testSerialization()
        {
            string objSer = "{\"id\":1,\"name\":\"value\"}";
            string serialized = "{\"tableName\":\"tableName\",\"operation\":\"INSERT\",\"data\":[{\"id\":1,\"name\":\"value\"},{\"id\":2,\"name\":\"value2\"}]}";
            KafkaDataEvent kde = new KafkaDataEvent("tableName", Operation.INSERT, 
                new string[] { "{\"id\":1, \"name\":\"value\"}]", "{\"id\":2, \"name\":\"value2\"}]" });

            TestObject obj = new TestObject();
            TestObject obj2 = new TestObject() { id=2, name="value2"};
            var arr = new TestObject[] { obj, obj2 };

            string data = Kafka.AsString(obj);
            Assert.AreEqual(objSer, data);

            kde.data = arr;
            string text = Kafka.AsString(kde);
            Assert.AreEqual(serialized, text);
        }

        [TestMethod]
        public void testTupleSerialization()
        {
            string objSer = "{\"id\":1,\"name\":\"value\"}";
            string serialized = "{\"tableName\":\"tableName\",\"operation\":\"INSERT\",\"data\":[{\"id\":1,\"name\":\"value\"},{\"id\":2,\"name\":\"value2\"}]}";
            KafkaDataEvent kde = new KafkaDataEvent("tableName", Operation.INSERT, 
                new string[] { "{\"id\":1, \"name\":\"value\"}", "{\"id\":2, \"name\":\"value2\"}" });

            Dictionary<string, object> obj = new Dictionary<string, object>();
            obj["id"] = 1;
            obj["name"] = "value";

            string data = Kafka.AsString(obj);
            Assert.AreEqual(objSer, data);

            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2["id"] = 2;
            obj2["name"] = "value2";
            kde.data = new Dictionary<string,object>[]{ obj, obj2};
            string text = Kafka.AsString(kde);
            Assert.AreEqual(serialized, text);
        }

    }

    class TestObject
    {
        public long id { get; set; } = 1;
        public string name { get; set; } = "value";
    }
}
