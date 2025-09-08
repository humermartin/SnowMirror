using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MirrorRepository.Model.Kafka
{

    public enum Operation { INSERT, UPDATE, DELETE }
    public class KafkaDataEvent
    {
#pragma warning disable IDE1006 // Naming Styles
        public string tableName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Operation operation { get; set; } = Operation.INSERT;

        public object[] data { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public KafkaDataEvent()
        {

        }

        public KafkaDataEvent(string tableName, Operation op)
        {
            this.tableName = tableName;
            this.operation = op;
        }
        
        public KafkaDataEvent(string tableName, Operation op, object[] data)
        {
            this.tableName = tableName;
            this.operation = op;
            this.data = data;
        }
    }

}
