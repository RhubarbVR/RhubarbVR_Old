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
    public class SyncStream<T> : UserStream, DriveMember<T>, IWorldObject, ISyncMember
    {
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
        private T _value;


        public T value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (!isDriven)
                {
                    UpdateValue();
                }
                onChangeInternal(this);
            }
        }

        private void UpdateValue()
        {
            DataNodeGroup obj = new DataNodeGroup();
            IDataNode Value;
            if (typeof(T).IsEnum)
            {
                Value = new DataNode<int>((int)(object)_value);
            }
            else
            {
                Value = new DataNode<T>(_value);
            }
            obj.setValue("Value", Value);
            world.netModule?.addToQueue(Net.ReliabilityLevel.Unreliable, obj, referenceID.id);
        }
        public SyncStream()
        {

        }

        public SyncStream(World _world, IWorldObject _parent, bool newref = true) : base(_world, _parent, newref)
        {

        }

        public SyncStream(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent, newref)
        {

        }

        public override DataNodeGroup serialize(bool netsync = false)
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            IDataNode Value;
            if (typeof(T).IsEnum)
            {
                Value = new DataNode<int>((int)(object)_value);
            }
            else
            {
                Value = new DataNode<T>(_value);
            }
            obj.setValue("Value", Value);
            obj.setValue("Name", name.serialize());
            return obj;
        }

        public override void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log($"Node did not exsets When loading Sync Value { this.GetType().FullName}");
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
            if (typeof(T).IsEnum)
            {
                _value = (T)(object)((DataNode<int>)data.getValue("Value")).Value;
            }
            else
            {
                _value = ((DataNode<T>)data.getValue("Value")).Value;
            }
            DataNodeGroup dataNode = (DataNodeGroup)data.getValue("Name");
            name.deSerialize(dataNode, onload, NewRefIDs, newRefID, latterResign);

        }

        public void ReceiveData(DataNodeGroup data, Peer peer)
        {
            if (typeof(T).IsEnum)
            {
                _value = (T)(object)((DataNode<int>)data.getValue("Value")).Value;
            }
            else
            {
                _value = ((DataNode<T>)data.getValue("Value")).Value;
            }
        }
    }
}
