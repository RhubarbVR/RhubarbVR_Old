using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
    public class SyncObjList<T> : Worker, IWorldObject, ISyncMember where T : Worker, new()
    {
        private SynchronizedCollection<T> _synclist = new SynchronizedCollection<T>(25);

        public T this[int i]
        {
            get
            {
                return _synclist[i];
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _synclist.GetEnumerator();
        }

        public List<T> getCopy()
        {
            return new List<T>(_synclist);
        }

        public int Count()
        {
            return _synclist.Count;
        }

        public void AddInternal(T value)
        {
            _synclist.SafeAdd(value);
            value.onDispose += Value_onDispose;
            addDisposable(value);
        }

        public int GetIndexOf(T val)
        {
           return _synclist.IndexOf(val);
        }

        private void Value_onDispose(Worker worker)
        {
            try
            {
                _synclist.Remove((T)worker);
            }
            catch
            {

            }
        }

        public void RemoveInternal(T value)
        {
            _synclist.Remove(value);
            value.onDispose -= Value_onDispose;
            removeDisposable(value);
        }

        public T Add(bool Refid = true)
        {
            T a = new T();
            a.initialize(this.world, this, Refid);
            AddInternal(a);
            if (Refid)
            {
                netAdd(a);
            }
            return _synclist[_synclist.Count - 1];
        }

        public void Clear()
        {
            _synclist.Clear();
            netClear();
        }

        private void netAdd(T val)
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(0));
            DataNodeGroup tip = val.serialize(true);
            send.setValue("Data", tip);
            world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        private void netClear()
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(1));
            world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        public void ReceiveData(DataNodeGroup data, Peer peer)
        {
            if (((DataNode<byte>)data.getValue("Type")).Value == 1)
            {
                _synclist.Clear();
            }
            else
            {
                T a = new T();
                a.initialize(this.world, this, false);
                List<Action> actions = new List<Action>();
                a.deSerialize((DataNodeGroup)data.getValue("Data"), actions, false);
                _synclist.SafeAdd(a);
                foreach (var item in actions)
                {
                    item?.Invoke();
                }
            }
        }

        public SyncObjList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncObjList(IWorldObject _parent,bool refid=true) : base(_parent.World, _parent, refid)
        {

        }
        public override DataNodeGroup serialize(bool netsync = false)
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach (T val in _synclist)
            {
                DataNodeGroup tip = val.serialize(netsync);
                if (tip != null)
                {
                    list.Add(tip);
                }
            }
            obj.setValue("list", list);
            return obj;
        }
        public override void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncObjList");
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
                Add(NewRefIDs).deSerialize(val, onload,NewRefIDs, newRefID, latterResign);
            }
        }
    }
}
