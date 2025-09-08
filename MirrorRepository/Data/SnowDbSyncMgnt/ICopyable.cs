using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    public interface ICopyable<TEntity> where TEntity : class
    {
        TEntity Copy();
    }

}
