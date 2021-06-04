using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class Sync<T> : Worker, DriveMember<T>, IWorldObject
    {
        public IDriver drivenFromobj;
        public NetPointer drivenFrom { get { return drivenFromobj.ReferenceID; } }

        public bool isDriven { get; private set; }

        private List<Driveable> driven = new List<Driveable>();

        public override void Removed()
        {
            foreach(Driveable dev in driven)
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
                onChangeInternal(this);
            }
        }

        public Sync(World _world, IWorldObject _parent,bool newref = true) : base(_world, _parent, newref)
        {

        }

        public Sync(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent,newref)
        {

        }

        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<T> Value = new DataNode<T>(_value);
            obj.setValue("Value", Value);
            return obj;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading Sync Value");
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
            _value = ((DataNode<T>)data.getValue("Value")).Value;
        }
    }
}
