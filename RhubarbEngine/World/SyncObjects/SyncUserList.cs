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
	public class SyncUserList : Worker, IWorldObject, ISyncMember
	{
		public SyncUserList()
		{

		}
		public SyncUserList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public SyncUserList(IWorldObject _parent) : base(_parent.World, _parent)
		{

		}

		public SyncUserList(IWorldObject _parent, bool newrefid = true) : base(_parent.World, _parent, newrefid)
		{

		}

		private readonly SynchronizedCollection<User> _synclist = new(5);
		[NoSave]
		[NoSync]
		[NoShow]
		public User this[int i]
		{
			get
			{
				return _synclist[i];
			}
		}
		public IEnumerator<User> GetEnumerator()
		{
			for (var i = 0; i < _synclist.Count; i++)
			{
				yield return this[i];
			}
		}
		public int Count()
		{
			return _synclist.Count;
		}
		[NoShow]
		[NoSave]
		[NoSync]
		public User Add(bool Refid = true)
		{
			var a = new User();
			a.Initialize(World, this, Refid);
			_synclist.SafeAdd(a);
			if (Refid)
			{
				NetAdd(a);
			}
			return a;
		}

		public void Clear()
		{
			_synclist.Clear();
			NetClear();
		}

		private void NetAdd(User val)
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(0));
			var tip = val.Serialize(new WorkerSerializerObject(true));
			send.SetValue("Value", tip);
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, ReferenceID.id);
		}

		private void NetClear()
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(1));
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, ReferenceID.id);
		}

		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (((DataNode<byte>)data.GetValue("Type")).Value == 1)
			{
				_synclist.Clear();
			}
			else
			{
				var a = new User();
				a.Initialize(World, this, false);
				var actions = new List<Action>();
				a.DeSerialize((DataNodeGroup)data.GetValue("Value"), actions, false);
				_synclist.SafeAdd(a);
				foreach (var item in actions)
				{
					item?.Invoke();
				}
			}
		}

		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
			return workerSerializerObject.CommonListSerialize(this,_synclist.Cast<IWorldObject>());
		}
		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				World.worldManager.Engine.Logger.Log("Node did not exsets When loading SyncUserList");
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
			foreach (DataNodeGroup val in ((DataNodeList)data.GetValue("list")))
			{

				Add(NewRefIDs).DeSerialize(val, onload, NewRefIDs, newRefID, latterResign);
			}
		}
	}
}
