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

	public class SyncRef<T> : Worker, ISyncRef, IDriveMember<NetPointer>, IWorldObject, ISyncMember where T : class, IWorldObject
	{
        public bool Driven
        {
            get
            {
                return IsDriven;
            }
        }

        private NetPointer _targetRefID;

		private T _target;

		public IWorldObject TargetIWorldObject { get { return Target; } set { if (value != null) { Value = value.ReferenceID; } } }

		public virtual T Target
		{
			get
			{
                return this._target == null || this._target.IsRemoved || this._target.World != World ? null : this._target;
            }
            set
			{
				_target = value;
				_targetRefID = value == null ? default : value.ReferenceID;
                Bind();
				UpdateNetValue();
				Change();
				OnChangeInternal(this);
			}
		}

		public virtual void Change()
		{

		}

		private void UpdateNetValue()
		{
			if (!IsDriven)
			{
				var send = new DataNodeGroup();
				send.SetValue("Value", new DataNode<NetPointer>(_targetRefID));
				UpdateNetIngect(send);
				World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, ReferenceID.id);
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
				_target = (T)World.GetWorldObj(thing);
				Bind();
			}
			catch
			{
				_target = null;
			}
			OnChangeInternal(this);
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
					_target = (T)World.GetWorldObj(value);
					Bind();
					UpdateNetValue();
				}
				catch
				{
					_target = null;
				}
				OnChangeInternal(this);
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
					_target = (T)World.GetWorldObj(value);
					Bind();
				}
				catch
				{
					Logger.Log("Failed To loaded" + _targetRefID.id.ToString());
					_target = null;
				}
				OnChangeInternal(this);
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
		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
            var obj = WorkerSerializerObject.CommonRefSerialize(this, _targetRefID);
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
				World.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
				return;
			}
			_temp = ((DataNode<NetPointer>)data.GetValue("targetRefID")).Value;
			if (NewRefIDs)
			{
				newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID(), ReferenceID.getID());
				if (latterResign.ContainsKey(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()))
				{
					foreach (var func in latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()])
					{
						func(ReferenceID.getID());
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
				ReferenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				World.AddWorldObj(this);
			}
			onload.Insert(0, LoadRefPoint);
			ReceiveDataIngect(data);
		}

		public IDriver drivenFromobj;
		public NetPointer DrivenFrom { get { return drivenFromobj.ReferenceID; } }

		public bool IsDriven { get; private set; }

		private readonly List<IDriveable> _driven = new();

		public override void Removed()
		{
			foreach (var dev in _driven)
			{
				dev.KillDrive();
			}
		}

		public void KillDrive()
		{
			drivenFromobj.RemoveDriveLocation();
			IsDriven = false;
		}

		public void Drive(IDriver value)
		{
			if (!IsDriven)
			{
				ForceDrive(value);
			}
		}
		public void ForceDrive(IDriver value)
		{
			if (IsDriven)
			{
				KillDrive();
			}
			value.SetDriveLocation(this);
			drivenFromobj = value;
			IsDriven = true;
		}


	}
}
