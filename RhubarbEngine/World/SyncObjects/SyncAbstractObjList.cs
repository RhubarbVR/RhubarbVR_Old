using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.World
{
    public class SyncAbstractObjList<T> : Worker, IWorldObject where T : Worker
    {
        private List<T> _synclist = new List<T>();

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
        public T Add(T val,bool Refid = true)
        {
            val.initialize(this.world, this, Refid);
            _synclist.Add(val);
            return _synclist[_synclist.Count - 1];
        }

        public void Clear()
        {
            _synclist.Clear();
        }
        public SyncAbstractObjList(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncAbstractObjList(IWorldObject _parent,bool refid=true) : base(_parent.World, _parent, refid)
        {

        }
        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNodeList list = new DataNodeList();
            foreach (T val in _synclist)
            {
                DataNodeGroup listobj = new DataNodeGroup();
                listobj.setValue("Value", val.serialize());
                //Need To add Constant Type Strings for better compression 
                listobj.setValue("Type",new DataNode<string>(val.GetType().FullName));
                list.Add(listobj);
            }
            obj.setValue("list", list);
            return obj;
        }
        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
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
                        Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), NewRefIDs, newRefID, latterResign);
                    }
                    else
                    {
                        if ((ty).IsAssignableFrom(typeof(T))) { 
                            T obj = (T)Activator.CreateInstance(ty);
                            Add(obj, NewRefIDs).deSerialize(((DataNodeGroup)((DataNodeGroup)val.getValue("Value")).getValue("Data")), NewRefIDs, newRefID, latterResign);
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
                            Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), NewRefIDs, newRefID, latterResign);
                        }
                    }
                    else
                    {
                        T obj = (T)Activator.CreateInstance(ty);
                        Add(obj, NewRefIDs).deSerialize((DataNodeGroup)val.getValue("Value"), NewRefIDs, newRefID, latterResign);
                    }
                }
            }
        }
    }
}
