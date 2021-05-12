using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class SyncRef<T> : Worker<SyncRef<T>>, IWorldObject where T : IWorldObject
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

        public void RefIDResign(RefID NewID) {
            targetRefID = NewID;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>), Dictionary<RefID, RefIDResign> latterResign = default(Dictionary<RefID, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            targetRefID = ((DataNode<RefID>)data.getValue("targetRefID")).Value;
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<RefID>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<RefID>)data.getValue("referenceID")).Value](referenceID);
                if (newRefID[targetRefID].getID()  != 0)
                {
                    targetRefID = newRefID[targetRefID];
                }
                else
                {
                    latterResign[targetRefID] = RefIDResign;
                }

            }
            else
            {
                referenceID = ((DataNode<RefID>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
        }
    }
}
