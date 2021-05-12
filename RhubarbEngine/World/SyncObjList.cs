using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class SyncObjList<T> : Worker, IWorldObject where T : Worker, new()
    {
        private List<T> _synclist = new List<T>();

        public T this[int i]
        {
            get
            {
                return _synclist[i];
            }
        }

        public int Count()
        {
            return _synclist.Count;
        }

        public T Add(bool Refid = true)
        {
            T a = new T();
            a.initialize(this.world, this, Refid);
            _synclist.Add(a);
            return _synclist[_synclist.Count - 1];
        }

        public void Clear()
        {
            _synclist.Clear();
        }
        public SyncObjList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncObjList(IWorldObject _parent,bool refid=true) : base(_parent.World, _parent, refid)
        {

        }
        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach (T val in _synclist)
            {
                list.Add(val.serialize());
            }
            obj.setValue("list", list);
            return obj;
        }
        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>), Dictionary<RefID, RefIDResign> latterResign = default(Dictionary<RefID, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncObjList");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<RefID>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<RefID>)data.getValue("referenceID")).Value](referenceID);
            }
            else
            {
                referenceID = ((DataNode<RefID>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
            {
                Add(NewRefIDs).deSerialize(val, NewRefIDs, newRefID, latterResign);
            }
        }
    }
}
