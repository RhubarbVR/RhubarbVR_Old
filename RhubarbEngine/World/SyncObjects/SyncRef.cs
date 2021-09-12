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
	public interface ISyncRef : IWorldObject
	{
		public bool Driven { get; }

		public NetPointer Value
		{
			get;
			set;
		}

		public IWorldObject TargetIWorldObject { get; set; }
	}

	public class SyncRef<T> : Worker, ISyncRef, DriveMember<NetPointer>, IWorldObject, ISyncMember where T : class, IWorldObject
	{
        public bool Driven
        {
            get
            {
                return isDriven;
            }
        }

        private NetPointer _targetRefID;

		private T _target;

		public IWorldObject TargetIWorldObject { get { return Target; } set { if (value != null) { Value = value.ReferenceID; } } }

		public virtual T Target
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
					_targetRefID = default;
				}
				else
				{
					_targetRefID = value.ReferenceID;
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
				var send = new DataNodeGroup();
				send.SetValue("Value", new DataNode<NetPointer>(_targetRefID));
				UpdateNetIngect(send);
				world.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
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
			var thing = ((DataNode<NetPointer>)data.GetValue("Value")).Value;
			ReceiveDataIngect(data);
			try
			{
				_targetRefID = thing;
				_target = (T)world.GetWorldObj(thing);
				Bind();
			}
			catch
			{
				_target = null;
			}
			onChangeInternal(this);
		}
		public virtual NetPointer Value
		{
			get
			{
				return _targetRefID;
			}
			set
			{
				try
				{
					_targetRefID = value;
					_target = (T)world.GetWorldObj(value);
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

		private NetPointer Netvalue
		{
			get
			{
				return _targetRefID;
			}
			set
			{
				try
				{
					_targetRefID = value;
					_target = (T)world.GetWorldObj(value);
					Bind();
				}
				catch
				{
					logger.Log("Failed To loaded" + _targetRefID.id.ToString());
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

		public SyncRef(IWorldObject _parent, bool newrefid = true) : base(_parent.World, _parent, newrefid)
		{

		}
		public override DataNodeGroup Serialize(bool netsync = false)
		{
			var obj = new DataNodeGroup();
			var Refid = new DataNode<NetPointer>(referenceID);
			obj.SetValue("referenceID", Refid);
			var Value = new DataNode<NetPointer>(_targetRefID);
			obj.SetValue("targetRefID", Value);
			UpdateNetIngect(obj);
			return obj;
		}

		public void RefIDResign(ulong NewID)
		{
			_temp = new NetPointer(NewID);
		}

		private void LoadRefPoint()
		{
			Netvalue = _temp;
		}

		private NetPointer _temp;

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
				return;
			}
			_temp = ((DataNode<NetPointer>)data.GetValue("targetRefID")).Value;
			if (NewRefIDs)
			{
				newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID(), referenceID.getID());
				if (latterResign.ContainsKey(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()))
				{
					foreach (var func in latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()])
					{
						func(referenceID.getID());
					}
				}
				if (newRefID.ContainsKey(_temp.getID()))
				{
					_temp = new NetPointer(newRefID[_temp.getID()]);
				}
				else
				{
					if (!latterResign.ContainsKey(_temp.getID()))
					{
						latterResign.Add(_temp.getID(), new List<RefIDResign>());
					}
					latterResign[_temp.getID()].Add(RefIDResign);
				}

			}
			else
			{
				referenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				world.AddWorldObj(this);
			}
			onload.Insert(0, LoadRefPoint);
			ReceiveDataIngect(data);
		}

		public IDriver drivenFromobj;
		public NetPointer drivenFrom { get { return drivenFromobj.ReferenceID; } }

		public bool isDriven { get; private set; }

		private readonly List<Driveable> _driven = new List<Driveable>();

		public override void Removed()
		{
			foreach (var dev in _driven)
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
