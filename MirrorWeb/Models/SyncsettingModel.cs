using System;
using System.Collections.Generic;
using MirrorRepository.Model;
using MirrorRepository.SnowTableApi;

namespace MirrorWeb.Models
{
    public class SyncSettingModel
    {
        public Guid? SynchronizationId { get; set; }
        public string SynchronizationName { get; set; }

        public bool BulkSync { get; set; }

        public List<SnowTables> SyncTableList { get; set; }

        public string SyncTableName { get; set; }

        public bool AutoSchemaUpdate { get; set; }

        public List<RestSchemaResponse> SnowColumns { get; set; }

        public Guid InstanceId { get; set; }

        public Guid DatabaseId { get; set; }
    }
}