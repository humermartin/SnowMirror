using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Base
{
    public class LogMessage
    {
        public string Key { get; set; }
        public string Table { get; set; }
        public string Message { get; set; }

        public static string Serialize(List<LogMessage> messages)
        {
            if (messages == null || messages.Count == 0) return null;
            try
            {
                return JsonConvert.SerializeObject(messages);
            }
            catch { }
            return null;
        }
        public static List<LogMessage> Deserialize(string messages)
        {
            if (string.IsNullOrEmpty(messages)) return new List<LogMessage>();
            try
            {
                return JsonConvert.DeserializeObject<LogMessage[]>(messages).ToList();
            }
            catch { }
            return new List<LogMessage>();
        }

        public override string ToString()
        {
            return GetType().Name + "[" + Key + "] table=" + Table;
        }
    }
}
