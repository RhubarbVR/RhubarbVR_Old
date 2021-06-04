using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class SyncRef<T> : Worker, IWorldObject where T : class,IWorldObject
    {
        private NetPointer targetRefID;

        private T _target;

        public T target
        {
            get
            {
                if (this._target == null || this._target.IsRemoved)
                {
                    return null;
                }
                return this._target;
            }
            set
            {
                _target = value;
                if (value == null)
                {
                    targetRefID = default(NetPointer);
                    return;
                }
                targetRefID = value.ReferenceID;

                onChangeInternal(this);
            }
        }

        public NetPointer value
        {
            get
            {
                return targetRefID;
            }
            set
            {
                target = (T)world.getWorldObj(value);
                onChangeInternal(this);
            }
        }
        public SyncRef(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public SyncRef(IWorldObject _parent) : base(_parent.World, _parent)
        {

        }

        public SyncRef(IWorldObject _parent,bool newrefid = true) : base(_parent.World, _parent, newrefid)
        {

        }
        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<NetPointer> Value = new DataNode<NetPointer>(targetRefID);
            obj.setValue("targetRefID", Value);
            return obj;
        }

        public void RefIDResign(ulong NewID) {
            targetRefID = new NetPointer(NewID);
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            targetRefID = ((DataNode<NetPointer>)data.getValue("targetRefID")).Value;
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
                if (newRefID.ContainsKey(targetRefID.getID()))
                {
                    targetRefID = new NetPointer(newRefID[targetRefID.getID()]);
                }
                else
                {
                    if (!latterResign.ContainsKey(targetRefID.getID()))
                    {
                        latterResign.Add(targetRefID.getID(), new List<RefIDResign>());
                    }
                    latterResign[targetRefID.getID()].Add(RefIDResign);
                }

            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
        }
    }
}
