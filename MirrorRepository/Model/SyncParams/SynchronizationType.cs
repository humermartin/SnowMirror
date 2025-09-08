using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Model.SyncParams
{
    public class SynchronizationType
    {
        public Guid SyncTypeId { get; set; }

        public string SyncTypeName { get; set; }

        public SyncParameter SyncParameter { get; set; }
    }
}
