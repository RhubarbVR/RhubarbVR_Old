using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class SyncValueList<T>: Worker, IWorldObject
    {
        private List<Sync<T>> _synclist = new List<Sync<T>>();

        Sync<T> this[int i]
        {
            get
            {
                return _synclist[i];
            }
        }

        public Sync<T> Add(bool Refid = true)
        {
            _synclist.Add(new Sync<T>(this.world, this, Refid));
            return _synclist[_synclist.Count - 1];
        }

        public void Clear()
        {
            _synclist.Clear();
        }
        public SyncValueList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncValueList(IWorldObject _parent) : base(_parent.World, _parent)
        {

        }
        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach (Sync<T> val in _synclist)
            {
                list.Add(val.serialize());
            }
            obj.setValue("list", list);
            return obj;
        }
        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<NetPointer, NetPointer> newRefID = default(Dictionary<NetPointer, NetPointer>), Dictionary<NetPointer, RefIDResign> latterResign = default(Dictionary<NetPointer, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value](referenceID);
            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
            {
                Add(NewRefIDs).deSerialize(val, NewRefIDs, newRefID, latterResign);
            }
        }
    }
}
