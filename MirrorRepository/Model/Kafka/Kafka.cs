using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace MirrorRepository.Model.Kafka
{
    public class Kafka
    {

        public static string AsString(object obj)
        {
            return AsStringBuilder(obj).ToString();
        }

        public static StringBuilder AsStringBuilder(object obj)
        {
            JsonSerializer ser = new JsonSerializer();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            ser.Serialize(sw, obj);
            return sb;
        }
    }
}
