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
	public class Sync<T> : Worker, IDriveMember<T>, IPrimitiveEditable where T : IConvertible
	{

		public IDriver drivenFromobj;

		public NetPointer DrivenFrom { get { return drivenFromobj.ReferenceID; } }

		public bool IsDriven { get; private set; }

		private readonly SynchronizedCollection<IDriveable> _driven = new();

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

		public virtual T Defalut()
		{
			return default;
		}

		private T _value;

		public virtual void UpdatedValue()
		{

		}
		public virtual void LoadedFromBytes(bool networked)
		{

		}
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
				UpdatedValue();
				OnChangeInternal(this);
			}
		}

		public string PrimitiveString { get { return Value?.ToString()??"null"; } set { SetValueAsString(value); } }

        public bool Driven
        {
            get
            {
                return IsDriven;
            }
        }

        public void SetValueAsString(string svalue)
		{
			try
			{
				var timvalue = (T)Convert.ChangeType(svalue, typeof(T));
				Value = timvalue;
			}
			catch
			{

			}
		}

		public void SetValueNoOnChange(T value)
		{
			_value = value;
			if (!IsDriven)
			{
				UpdateValue();
			}
			UpdatedValue();
		}

		private void UpdateValue()
		{
			var obj = new DataNodeGroup();
			var Value = typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : (IDataNode)new DataNode<T>(_value);
            obj.SetValue("Value", Value);
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, obj, ReferenceID.id);
		}

        public Sync()
        {

        }

        public Sync(World _world, IWorldObject _parent, bool newref = true, T val = default) : base(_world, _parent, newref)
		{
			Value = val;
		}

		public Sync(IWorldObject _parent, bool newref = true, T val = default) : base(_parent.World, _parent, newref)
		{
			Value = val;
		}

        public virtual T SaveToBytes()
        {
            return _value;
        }

        public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
            return WorkerSerializerObject.CommonValueSerialize(this, SaveToBytes());
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			_value = Defalut();
			if (data == null)
			{
				throw new Exception($"Node did not exsets When loading Sync Value { GetType().FullName}");
			}
			if (NewRefIDs)
			{
				newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.GetID(), ReferenceID.GetID());
				if (latterResign.ContainsKey(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.GetID()))
				{
					foreach (var func in latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.GetID()])
					{
						func(ReferenceID.GetID());
					}
				}
			}
			else
			{
				ReferenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				World.AddWorldObj(this);
			}
			_value = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data.GetValue("Value")).Value : ((DataNode<T>)data.GetValue("Value")).Value;
            LoadedFromBytes(NewRefIDs);
		}

		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (typeof(T).IsEnum)
			{
				_value = (T)(object)((DataNode<int>)data.GetValue("Value")).Value;
				OnChangeInternal(this);
			}
			else
			{
				_value = ((DataNode<T>)data.GetValue("Value")).Value;
				OnChangeInternal(this);
			}
		}
	}
}
