using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MirrorWeb.Models
{
    public class SnowTableObjectList
    {
        public string SnowInstance{ get; set; }

        public List<SnowTableObject> SnowTableObjects { get; set; }
    
        
    }
}