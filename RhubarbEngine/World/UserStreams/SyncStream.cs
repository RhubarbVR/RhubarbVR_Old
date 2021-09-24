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
	public class SyncStream<T> : UserStream, IDriveMember<T>, IWorldObject, ISyncMember where T : IConvertible
	{
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
				if (!IsDriven)
				{
					UpdateValue();
				}
				OnChangeInternal(this);
			}
		}

		private void UpdateValue()
		{
			var obj = new DataNodeGroup();
			var Value = typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : (IDataNode)new DataNode<T>(_value);
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
            var obj = WorkerSerializerObject.CommonValueSerialize(this, _value);
			obj.SetValue("Name", name.Serialize(new WorkerSerializerObject()));
			return obj;
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				Logger.Log($"Node did not exsets When loading Sync Value { GetType().FullName}");
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
			_value = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data.GetValue("Value")).Value : ((DataNode<T>)data.GetValue("Value")).Value;
            var dataNode = (DataNodeGroup)data.GetValue("Name");
			name.DeSerialize(dataNode, onload, NewRefIDs, newRefID, latterResign);

		}

        void ISyncMember.ReceiveData(DataNodeGroup data, Peer peer)
		{
			_value = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data.GetValue("Value")).Value : ((DataNode<T>)data.GetValue("Value")).Value;
        }
	}
}
