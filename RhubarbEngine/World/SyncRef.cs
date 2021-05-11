using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public class SyncRef<T> : Worker where T : IWorldObject
    {

        private Sync<RefID> targetRefID;
        public SyncRef(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
    }
}
