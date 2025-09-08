using System;
using System.Collections.Generic;
using MirrorRepository.SnowTableApi.TableDefinitions;

namespace MirrorWeb.Models
{
    public class SysClusterStateViewModel
    {
        public List<SysClusterState> Nodes { get; set; }

        public int NodesTotalCount { get; set; }
        
    }
}