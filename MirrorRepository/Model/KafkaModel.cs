using System;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class KafkaModel
    {
        /// <summary>
        /// Gets or sets the sql user name
        /// </summary>
        public string EndpointUrl { get; set; }

        
    }
}
