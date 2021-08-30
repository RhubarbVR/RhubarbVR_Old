using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public interface ISyncList : IWorldObject, IEnumerable<IWorldObject>
    {
        public int Count();

        public bool TryToAddToSyncList();
    }
}
