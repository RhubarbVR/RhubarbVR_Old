using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class SyncRef<T> : Worker, IChangeable, IWorldObject where T : class,IWorldObject
    {
        public event Action<IChangeable> Changed;

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

        public void RefIDResign(NetPointer NewID) {
            targetRefID = NewID;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<NetPointer, NetPointer> newRefID = default(Dictionary<NetPointer, NetPointer>), Dictionary<NetPointer, RefIDResign> latterResign = default(Dictionary<NetPointer, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            targetRefID = ((DataNode<NetPointer>)data.getValue("targetRefID")).Value;
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value](referenceID);
                if (newRefID[targetRefID].getID()  != 0)
                {
                    targetRefID = newRefID[targetRefID];
                }
                else
                {
                    latterResign[targetRefID] = RefIDResign;
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
