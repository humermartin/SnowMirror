using System.Collections.Generic;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.Models
{
    public class TableMetaDataModel
    {

        public string TableName { get; set; }

        public List<Synchronization> Synchronizations { get; set; }

    }
}