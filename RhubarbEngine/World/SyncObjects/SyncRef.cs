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
    public class SyncRef<T> : Worker, DriveMember<NetPointer> ,IWorldObject, ISyncMember where T : class,IWorldObject
    {
        private NetPointer targetRefID;

        private T _target;

        public virtual T target
        {
            get
            {
                if (this._target == null || this._target.IsRemoved || this._target.World != world)
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
                }
                else
                {
                    targetRefID = value.ReferenceID;
                }
                Bind();
                UpdateNetValue();
                Change();
                onChangeInternal(this);
            }
        }

        public virtual void Change()
        {

        }

        private void UpdateNetValue()
        {
            if (!isDriven)
            {
                DataNodeGroup send = new DataNodeGroup();
                send.setValue("Value", new DataNode<NetPointer>(targetRefID));
                UpdateNetIngect(send);
                world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
            }
        }

        public virtual void UpdateNetIngect(DataNodeGroup data)
        {

        }

        public virtual void ReceiveDataIngect(DataNodeGroup data)
        {

        }

        public void ReceiveData(DataNodeGroup data, Peer peer)
        {
            NetPointer thing = ((DataNode<NetPointer>)data.getValue("Value")).Value;
            ReceiveDataIngect(data);
            try
            {
                targetRefID = thing;
                _target = (T)world.getWorldObj(thing);
                Bind();
            }
            catch
            {
                _target = null;
            }
            onChangeInternal(this);
        }
        public virtual NetPointer value
        {
            get
            {
                return targetRefID;
            }
            set
            {
                try
                {
                    targetRefID = value;
                    _target = (T)world.getWorldObj(value);
                    Bind();
                    UpdateNetValue();
                }
                catch
                {
                    _target = null;
                }
                onChangeInternal(this);
            }
        }

        public virtual void Bind()
        {

        }

        private NetPointer _netvalue
        {
            get
            {
                return targetRefID;
            }
            set
            {
                try
                {
                    targetRefID = value;
                    _target = (T)world.getWorldObj(value);
                    Bind();
                }
                catch
                {
                    logger.Log("Failed To loaded" + targetRefID.id.ToString());
                    _target = null;
                }
                onChangeInternal(this);
            }
        }

        public SyncRef()
        {

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
        public override DataNodeGroup serialize(bool netsync = false)
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<NetPointer> Value = new DataNode<NetPointer>(targetRefID);
            obj.setValue("targetRefID", Value);
            UpdateNetIngect(obj);
            return obj;
        }

        public void RefIDResign(ulong NewID) {
            temp = new NetPointer(NewID);
        }

        private void loadRefPoint()
        {
            _netvalue = temp;
        }

        private NetPointer temp;

        public override void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
                return;
            }
            temp = ((DataNode<NetPointer>)data.getValue("targetRefID")).Value;
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
                if (newRefID.ContainsKey(temp.getID()))
                {
                    temp = new NetPointer(newRefID[temp.getID()]);
                }
                else
                {
                    if (!latterResign.ContainsKey(temp.getID()))
                    {
                        latterResign.Add(temp.getID(), new List<RefIDResign>());
                    }
                    latterResign[temp.getID()].Add(RefIDResign);
                }

            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            onload.Insert(0, loadRefPoint);
            ReceiveDataIngect(data);
        }

        public IDriver drivenFromobj;
        public NetPointer drivenFrom { get { return drivenFromobj.ReferenceID; } }

        public bool isDriven { get; private set; }

        private List<Driveable> driven = new List<Driveable>();

        public override void Removed()
        {
            foreach (Driveable dev in driven)
            {
                dev.killDrive();
            }
        }

        public void killDrive()
        {
            drivenFromobj.RemoveDriveLocation();
            isDriven = false;
        }

        public void drive(IDriver value)
        {
            if (!isDriven)
            {
                forceDrive(value);
            }
        }
        public void forceDrive(IDriver value)
        {
            if (isDriven)
            {
                killDrive();
            }
            value.SetDriveLocation(this);
            drivenFromobj = value;
            isDriven = true;
        }


    }
}
