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
    public class SyncUserList : Worker, IWorldObject, ISyncMember
    {
        public SyncUserList()
        {

        }
        public SyncUserList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncUserList(IWorldObject _parent) : base(_parent.World, _parent)
        {

        }

        public SyncUserList(IWorldObject _parent, bool newrefid = true) : base(_parent.World, _parent, newrefid)
        {

        }

        private SynchronizedCollection<User> _synclist = new SynchronizedCollection<User>(5);
        [NoSave]
        [NoSync]
        public User this[int i]
        {
            get
            {
                return _synclist[i];
            }
        }
        public IEnumerator<User> GetEnumerator()
        {
            for (int i = 0; i < _synclist.Count; i++)
            {
                yield return this[i];
            }
        }
        public int Count()
        {
            return _synclist.Count;
        }
        [NoSave]
        [NoSync]
        public User Add(bool Refid = true)
        {
            User a = new User();
            a.initialize(this.world, this, Refid);
            _synclist.SafeAdd(a);
            if (Refid)
            {
                netAdd(a);
            }
            return a;
        }

        public void Clear()
        {
            _synclist.Clear();
            netClear();
        }

        private void netAdd(User val)
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(0));
            DataNodeGroup tip = val.serialize(true);
            send.setValue("Value", tip);
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
                User a = new User();
                a.initialize(this.world, this, false);
                List<Action> actions = new List<Action>();
                a.deSerialize(((DataNodeGroup)data.getValue("Value")), actions, false);
                _synclist.SafeAdd(a);
                foreach (var item in actions)
                {
                    item?.Invoke();
                }
            }
        }

        public override DataNodeGroup serialize(bool netsync = false)
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach (User val in _synclist)
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
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncUserList");
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
