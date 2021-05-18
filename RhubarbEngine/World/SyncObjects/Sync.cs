using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class Sync<T> : Worker, DriveMember<T>, IWorldObject
    {
        public event Action<IChangeable> Changed;

        public IDriver drivenFromobj;
        public RefID drivenFrom { get { return drivenFromobj.ReferenceID; } }

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
            DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<T> Value = new DataNode<T>(_value);
            obj.setValue("Value", Value);
            return obj;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>), Dictionary<RefID, RefIDResign> latterResign = default(Dictionary<RefID, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading Sync Value");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<RefID>)data.getValue("referenceID")).Value, referenceID);
                latterResign[((DataNode<RefID>)data.getValue("referenceID")).Value](referenceID);
            }
            else
            {
                referenceID = ((DataNode<RefID>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            _value = ((DataNode<T>)data.getValue("Value")).Value;
        }
    }
}
