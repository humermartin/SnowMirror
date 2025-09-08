using MirrorRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Processor
{
    public interface SyncTask
    {
        string GetKey();
        int IntervalInMinutes { get; set; }
        SnowTables Table { get; }

        void Execute();
     }
}
