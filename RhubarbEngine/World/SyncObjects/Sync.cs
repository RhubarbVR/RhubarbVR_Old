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
	public class Sync<T> : Worker, DriveMember<T>, IPrimitiveEditable where T : IConvertible
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
				if (!isDriven)
				{
					UpdateValue();
				}
				UpdatedValue();
				onChangeInternal(this);
			}
		}

		public string primitiveString { get { return Value.ToString(); } set { SetValueAsString(value); } }

        public bool Driven
        {
            get
            {
                return isDriven;
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
			if (!isDriven)
			{
				UpdateValue();
			}
			UpdatedValue();
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
			world.NetModule?.AddToQueue(Net.ReliabilityLevel.LatestOnly, obj, referenceID.id);
		}

		public Sync(World _world, IWorldObject _parent, bool newref = true, T val = default) : base(_world, _parent, newref)
		{
			Value = val;
		}

		public Sync(IWorldObject _parent, bool newref = true, T val = default) : base(_parent.World, _parent, newref)
		{
			Value = val;
		}

		public virtual T SaveToBytes(bool netsync)
		{
			return _value;
		}

        public override DataNodeGroup Serialize(bool netsync = false)
		{
			var obj = new DataNodeGroup();
			var Refid = new DataNode<NetPointer>(referenceID);
			obj.SetValue("referenceID", Refid);
			IDataNode Value;
			if (typeof(T).IsEnum)
			{
				Value = new DataNode<int>((int)(object)SaveToBytes(netsync));
			}
			else
			{
				Value = new DataNode<T>(SaveToBytes(netsync));
			}
			obj.SetValue("Value", Value);
			return obj;
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			_value = Defalut();
			if (data == null)
			{
				world.worldManager.engine.logger.Log($"Node did not exsets When loading Sync Value { GetType().FullName}");
				return;
			}
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
			}
			else
			{
				referenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				world.AddWorldObj(this);
			}
			if (typeof(T).IsEnum)
			{
				_value = (T)(object)((DataNode<int>)data.GetValue("Value")).Value;
			}
			else
			{
				_value = ((DataNode<T>)data.GetValue("Value")).Value;
			}
			LoadedFromBytes(NewRefIDs);
		}

		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (typeof(T).IsEnum)
			{
				_value = (T)(object)((DataNode<int>)data.GetValue("Value")).Value;
				onChangeInternal(this);
			}
			else
			{
				_value = ((DataNode<T>)data.GetValue("Value")).Value;
				onChangeInternal(this);
			}
		}
	}
}
