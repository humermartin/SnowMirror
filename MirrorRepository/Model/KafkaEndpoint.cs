using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Model
{
    public class KafkaEndpoint
    {
        public string EndpointUrl { get; set; }

        public KafkaUser KafkaUser { get; set; }
    }
}
