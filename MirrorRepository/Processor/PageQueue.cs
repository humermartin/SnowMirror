using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Processor
{
    /// <summary>
    /// Queue of Pages (blocks of Snow-table tuples. 
    /// On failure to synchronize the block index will be returned for later retry..
    /// </summary>
    public class PageQueue
    {
        public string Table { get; internal set; }
        private int currentPage = -1;
        private Queue<QueueEntry> _retryPages = new Queue<QueueEntry>();
        public List<QueueEntry> FailedPages { get; private set; } = new List<QueueEntry>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public QueueEntry GetPage()
        {
            if (_retryPages.Count > 0)
            {
                return _retryPages.Dequeue();
            }
            return new QueueEntry() { Page = ++currentPage };
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReturnPage(QueueEntry pageIdx)
        {
            pageIdx.Failures++;
            _retryPages.Enqueue(pageIdx);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public QueueEntry GetMaxFails()
        {
            return _retryPages.OrderByDescending(p => p.Failures).FirstOrDefault();
        }

        public List<QueueEntry> RetryPages
        {
            get { return _retryPages.OrderBy(e => e.Page).ToList(); }
        }

    }

}
