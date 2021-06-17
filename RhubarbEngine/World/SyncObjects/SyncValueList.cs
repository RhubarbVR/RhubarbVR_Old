using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class SyncValueList<T>: Worker, IWorldObject
    {
        private List<Sync<T>> _synclist = new List<Sync<T>>();

        public int Count => _synclist.Count;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _synclist.Count; i++)
            {
                yield return this[i].value;
            }
        }
        public IEnumerator<Sync<T>> GetSyncEnumerator()
        {
            for (int i = 0; i < _synclist.Count; i++)
            {
                yield return this[i];
            }
        }
        public Sync<T> this[int i]
        {
            get
            {
                return _synclist[i];
            }
        }

        public Sync<T> Add(bool Refid = true)
        {
            var val = new Sync<T>(this.world, this, Refid);
            _synclist.Add(val);
            val.Changed += Val_Changed;
            return val;
        }

        private void Val_Changed(IChangeable obj)
        {
            onChangeInternal(obj);
        }

        public void Clear()
        {
            _synclist.Clear();
        }
        public SyncValueList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncValueList(IWorldObject _parent, bool newRefID) : base(_parent.World, _parent, newRefID)
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
