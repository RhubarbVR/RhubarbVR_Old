using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

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
                DataNodeGroup tip = val.serialize();
                if (tip != null)
                {
                    list.Add(tip);
                }
            }
            obj.setValue("list", list);
            return obj;
        }
        public void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID(), referenceID.getID());
                if (latterResign.ContainsKey(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()))
                {
                    foreach (RefIDResign func in latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()])
                    {
                        func(referenceID.getID());
                    }
                }
            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
            {
                Add(NewRefIDs).deSerialize(val, onload, NewRefIDs, newRefID, latterResign);
            }
        }
    }
}
