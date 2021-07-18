using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class SyncRefList<T> : SyncObjList<SyncRef<T>>, IWorldObject where T : class, IWorldObject
    {

        public SyncRefList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
    }
}
