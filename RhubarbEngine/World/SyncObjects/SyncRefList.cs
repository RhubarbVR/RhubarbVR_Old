using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class SyncRefList<T> : Worker, IWorldObject where T : class, IWorldObject
    {
        private List<SyncRef<T>> _syncreflist = new List<SyncRef<T>>();

        SyncRef<T> this[int i]
        {
            get
            {
                return _syncreflist[i];
            }
        }

        public SyncRef<T> Add(bool RefID = true)
        {
            _syncreflist.Add(new SyncRef<T>(this, RefID));
            return _syncreflist[_syncreflist.Count - 1];
        }

        public void Clear()
        {
            _syncreflist.Clear();
        }
        public SyncRefList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }

        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach(SyncRef<T> val in _syncreflist)
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
