using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Processor
{
    public class QueueEntry
    {
        public int Page { get; set; }
        public int Failures { get; set; }
        public override string ToString()
        {
            return "Page:" + Page + ", Failures:" + Failures;
        }
    }

}
