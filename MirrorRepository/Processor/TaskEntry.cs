using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorRepository.Processor;
using MirrorRepository.REST;

namespace MirrorRepository.Processor
{
    public class TaskEntry
    {
        public SyncTask SyncTask { get; internal set; }
        public Task Task { get; internal set; }
    };
}
