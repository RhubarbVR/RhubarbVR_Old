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
	public class SyncStream<T> : UserStream, DriveMember<T>, IWorldObject, ISyncMember where T : IConvertible
	{
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
		private T _value;


		public T Value
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
				OnChangeInternal(this);
			}
		}

		private void UpdateValue()
		{
			var obj = new DataNodeGroup();
			IDataNode Value;
			if (typeof(T).IsEnum)
			{
				Value = new DataNode<int>((int)(object)_value);
			}
			else
			{
				Value = new DataNode<T>(_value);
			}
			obj.SetValue("Value", Value);
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Unreliable, obj, ReferenceID.id);
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

		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
            var obj = workerSerializerObject.CommonValueSerialize(this, _value);
			obj.SetValue("Name", name.Serialize(new WorkerSerializerObject()));
			return obj;
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				World.worldManager.engine.logger.Log($"Node did not exsets When loading Sync Value { GetType().FullName}");
				return;
			}
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
			}
			else
			{
				ReferenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				World.AddWorldObj(this);
			}
			if (typeof(T).IsEnum)
			{
				_value = (T)(object)((DataNode<int>)data.GetValue("Value")).Value;
			}
			else
			{
				_value = ((DataNode<T>)data.GetValue("Value")).Value;
			}
			var dataNode = (DataNodeGroup)data.GetValue("Name");
			name.DeSerialize(dataNode, onload, NewRefIDs, newRefID, latterResign);

		}

        void ISyncMember.ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (typeof(T).IsEnum)
			{
				_value = (T)(object)((DataNode<int>)data.GetValue("Value")).Value;
			}
			else
			{
				_value = ((DataNode<T>)data.GetValue("Value")).Value;
			}
		}
	}
}
