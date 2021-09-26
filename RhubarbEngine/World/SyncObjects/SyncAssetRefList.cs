using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using System.Collections;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
	public class SyncAssetRefList<T> : Worker, ISyncList, IWorldObject, ISyncMember where T : IAsset
	{
        public SyncAssetRefList() { }
        private readonly SynchronizedCollection<AssetRef<T>> _syncreflist = new (5);

		public AssetRef<T> this[int i]
		{
			get
			{
				return _syncreflist[i];
			}
		}

		public int Length { get { return _syncreflist.Count; } }

		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < _syncreflist.Count; i++)
			{
				yield return this[i].Asset;
			}
		}

		public int IndexOf(AssetProvider<T> val)
		{
			var returnint = -1;
			for (var i = 0; i < _syncreflist.Count; i++)
			{
				if (this[i].Target == val)
				{
					returnint = i;
					return returnint;
				}
			}
			return returnint;
		}


		private void OnLoad(T val)
		{
			loadChange?.Invoke(val);
		}

		public Action<T> loadChange;


		public AssetRef<T> Add(bool RefID = true)
		{
			var a = new AssetRef<T>(this, RefID);
			a.LoadChange += OnLoad;
			_syncreflist.SafeAdd(a);
			if (RefID)
			{
				NetAdd(a);
			}
			return a;
		}

		private void NetAdd(AssetRef<T> val)
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(0));
			var tip = val.Serialize(new WorkerSerializerObject(true));
			send.SetValue("Data", tip);
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
				_syncreflist.Clear();
			}
			else
			{
				var a = new AssetRef<T>(this, false);
				a.LoadChange += OnLoad;
				var actions = new List<Action>();
				a.DeSerialize((DataNodeGroup)data.GetValue("Data"), actions, false);
				foreach (var item in actions)
				{
					item?.Invoke();
				}
				_syncreflist.SafeAdd(a);
			}
		}

		public void Clear()
		{
			_syncreflist.Clear();
			NetClear();
		}

		public SyncAssetRefList(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent, newref)
		{

		}

		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
			return workerSerializerObject.CommonListSerialize(this,_syncreflist.Cast<IWorldObject>());
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				World.worldManager.Engine.Logger.Log("Node did not exsets When loading SyncAssetRefList");
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
			foreach (DataNodeGroup val in (DataNodeList)data.GetValue("list"))
			{
				Add(NewRefIDs).DeSerialize(val, onload, NewRefIDs, newRefID, latterResign);
			}
		}

		int ISyncList.Count()
		{
			return Length;
		}

		public bool TryToAddToSyncList()
		{
			try
			{
				Add();
				return true;
			}
			catch
			{
				return false;
			}
		}

		IEnumerator<IWorldObject> IEnumerable<IWorldObject>.GetEnumerator()
		{
			foreach (var item in _syncreflist)
			{
				yield return (IWorldObject)item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IWorldObject>)this).GetEnumerator();
		}
		public void Remove(int index)
		{
			throw new NotSupportedException();
		}

	}
}
