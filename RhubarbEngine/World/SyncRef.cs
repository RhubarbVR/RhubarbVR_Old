using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;

namespace RhubarbEngine.World
{
    public class SyncRef<T> : Worker<SyncRef<T>> where T : IWorldObject
    {

        private RefID targetRefID;
        public SyncRef(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }

        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<RefID> Value = new DataNode<RefID>(targetRefID);
            obj.setValue("targetRefID", Value);
            return obj;
        }

        public void deSerialize(DataNodeGroup data)
        {

        }
    }
}
