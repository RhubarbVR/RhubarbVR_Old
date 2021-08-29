using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.Net;
using System.Collections;

namespace RhubarbEngine.World
{
    public class SyncAbstractObjList<T> : Worker,ISyncList ,IWorldObject, ISyncMember where T : Worker
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
            for (int i = 0; i < _synclist.Count; i++)
            {
                yield return this[i];
            }
        }
        public int Count()
        {
            return _synclist.Count;
        }
        public T Add(T val, bool Refid = true)
        {
            val.initialize(this.world, this, Refid);
            AddInternal(val);
            if (Refid)
            {
                netAdd(val);
            }
            return _synclist[_synclist.Count - 1];
        }
        public T Add<L>( bool Refid = true) where L:T
        {
            L val = (L)Activator.CreateInstance(typeof(L));
            val.initialize(this.world, this, Refid);
            AddInternal(val);
            if (Refid)
            {
                netAdd(val);
            }
            return _synclist[_synclist.Count - 1];
        }

        public void AddInternal(T value)
        {
            _synclist.SafeAdd(value);
            value.onDispose += Value_onDispose;
            addDisposable(value);
        }

        public void RemoveInternal(T value)
        {
            _synclist.Remove(value);
            value.onDispose -= Value_onDispose;
            removeDisposable(value);
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

        public T Add(Type type,bool Refid = true) 
        {
            T val = (T)Activator.CreateInstance(type);
            val.initialize(this.world, this, Refid);
            AddInternal(val);
            if (Refid)
            {
                netAdd(val);
            }
            return _synclist[_synclist.Count - 1];
        }

        private void netAdd(T val)
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(0));
            DataNodeGroup tip = val.serialize(true);
            DataNodeGroup listobj = new DataNodeGroup();
            if (tip != null)
            {
                listobj.setValue("Value", tip);
            }
            //Need To add Constant Type Strings for better compression 
            listobj.setValue("Type", new DataNode<string>(val.GetType().FullName));
            send.setValue("Data", listobj);
            world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        private void netClear()
        {
            DataNodeGroup send = new DataNodeGroup();
            send.setValue("Type", new DataNode<byte>(1));
            world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
        }

        public void Clear()
        {
            _synclist.Clear();
            netClear();
        }

        public void ReceiveData(DataNodeGroup data, Peer peer)
        {
            try
            {
                if (((DataNode<byte>)data.getValue("Type")).Value == 1)
                {
                    _synclist.Clear();
                }
                else
                {
                    Type ty = Type.GetType(((DataNode<string>)((DataNodeGroup)data.getValue("Data")).getValue("Type")).Value);
                    if (ty == null)
                    {
                        logger.Log("Type not found" + ((DataNode<string>)((DataNodeGroup)data.getValue("Data")).getValue("Type")).Value, true);
                    }
                    else
                    {
                        T val = (T)Activator.CreateInstance(ty);
                        val.initialize(this.world, this, false);
                        List<Action> actions = new List<Action>();
                        val.deSerialize((DataNodeGroup)((DataNodeGroup)data.getValue("Data")).getValue("Value"), actions, false);
                        _synclist.SafeAdd(val);
                        foreach (var item in actions)
                        {
                            item?.Invoke();
                        }
                    }
                }
            }catch(Exception e)
            {
                logger.Log("Error With net sync ab e:"+e.ToString());

            }

        }

        public SyncAbstractObjList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }

        public SyncAbstractObjList(IWorldObject _parent,bool refid=true) : base(_parent.World, _parent, refid)
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
                DataNodeGroup listobj = new DataNodeGroup();
                if (tip != null)
                {
                    listobj.setValue("Value", tip);
                }
                //Need To add Constant Type Strings for better compression 
                listobj.setValue("Type",new DataNode<string>(val.GetType().FullName));
                list.Add(listobj);
            }
            obj.setValue("list", list);
            return obj;
        }
        public override void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncAbstractObjList");
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
                Type ty = Type.GetType(((DataNode<string>)val.getValue("Type")).Value);
                if(ty == typeof(MissingComponent))
                {
                    ty = Type.GetType(((DataNode<string>)((DataNodeGroup)val.getValue("Value")).getValue("type")).Value, true);
                    if (ty == null)
                    {
                        world.worldManager.engine.logger.Log("Component still not found" + ((DataNode<string>)val.getValue("Type")).Value);
                        T obj = (T)Activator.CreateInstance(typeof(MissingComponent));
                        Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), onload, NewRefIDs, newRefID, latterResign);
                    }
                    else
                    {
                        if ((ty).IsAssignableFrom(typeof(T))) { 
                            T obj = (T)Activator.CreateInstance(ty);
                            Add(obj, NewRefIDs).deSerialize(((DataNodeGroup)((DataNodeGroup)val.getValue("Value")).getValue("Data")), onload, NewRefIDs, newRefID, latterResign);
                        }
                        else
                        {
                            world.worldManager.engine.logger.Log("Something is broken or someone is messing with things", true);
                        }
                    }
                }
                else
                {
                    if (ty == null)
                    {
                        world.worldManager.engine.logger.Log("Type not found" + ((DataNode<string>)val.getValue("Type")).Value,true);
                        if (typeof(T) == typeof(Component))
                        {
                            T obj = (T)Activator.CreateInstance(typeof(MissingComponent));
                            Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), onload, NewRefIDs, newRefID, latterResign);
                        }
                    }
                    else
                    {
                        T obj = (T)Activator.CreateInstance(ty);
                        Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), onload,NewRefIDs, newRefID, latterResign);
                    }
                }
            }
        }

        IEnumerator<IWorldObject> IEnumerable<IWorldObject>.GetEnumerator()
        {
            for (int i = 0; i < _synclist.Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IWorldObject>)this).GetEnumerator();
        }
    }
}
