using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;
using System.Collections;

namespace RhubarbEngine.World
{
	public class SyncObjList<T> : Worker, ISyncList, IWorldObject, ISyncMember where T : Worker, new()
	{
		private readonly SynchronizedCollection<T> _synclist = new SynchronizedCollection<T>(25);

		public T this[int i]
		{
			get
			{
				return _synclist[i];
			}
		}
		public IEnumerator<T> GetEnumerator()
		{
			return _synclist.GetEnumerator();
		}

		public List<T> GetCopy()
		{
			return new List<T>(_synclist);
		}

		public int Count()
		{
			return _synclist.Count;
		}

		public void AddInternal(T value)
		{
			_synclist.SafeAdd(value);
			value.onDispose += Value_onDispose;
			AddDisposable(value);
		}

		public int GetIndexOf(T val)
		{
			return _synclist.IndexOf(val);
		}

		private void Value_onDispose(Worker worker)
		{
			try
			{
				_synclist.Remove((T)worker);
			}
			catch
			{

			}
		}

		public void RemoveInternal(T value)
		{
			_synclist.Remove(value);
			value.onDispose -= Value_onDispose;
			RemoveDisposable(value);
		}

		public T Add(bool Refid = true)
		{
			var a = new T();
			a.initialize(world, this, Refid);
			AddInternal(a);
			if (Refid)
			{
				NetAdd(a);
			}
			return _synclist[_synclist.Count - 1];
		}

		public void Clear()
		{
			try
			{
				foreach (var item in _synclist.Reverse())
				{
					try
					{
						item.Dispose();
					}
					catch { }
				}
			}
			catch { }
			NetClear();
		}

		private void NetAdd(T val)
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(0));
			var tip = val.Serialize(true);
			send.SetValue("Data", tip);
			world.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
		}

		private void NetClear()
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(1));
			world.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
		}

		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (((DataNode<byte>)data.GetValue("Type")).Value == 1)
			{
				try
				{
					foreach (var item in _synclist.Reverse())
					{
						try
						{
							item.Dispose();
						}
						catch { }
					}
				}
				catch { }
			}
			else
			{
				var a = new T();
				a.initialize(world, this, false);
				var actions = new List<Action>();
				a.DeSerialize((DataNodeGroup)data.GetValue("Data"), actions, false);
				_synclist.SafeAdd(a);
				foreach (var item in actions)
				{
					item?.Invoke();
				}
			}
		}

		public SyncObjList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public SyncObjList(IWorldObject _parent, bool refid = true) : base(_parent.World, _parent, refid)
		{

		}
		public override DataNodeGroup Serialize(bool netsync = false)
		{
			var obj = new DataNodeGroup();
			var Refid = new DataNode<NetPointer>(referenceID);
			obj.SetValue("referenceID", Refid);
			var list = new DataNodeList();
			foreach (var val in _synclist)
			{
				var tip = val.Serialize(netsync);
				if (tip != null)
				{
					list.Add(tip);
				}
			}
			obj.SetValue("list", list);
			return obj;
		}
		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading SyncObjList");
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
			foreach (DataNodeGroup val in (DataNodeList)data.GetValue("list"))
			{
				Add(NewRefIDs).DeSerialize(val, onload, NewRefIDs, newRefID, latterResign);
			}
		}
		int ISyncList.Count()
		{
			return Count();
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
			foreach (var item in _synclist)
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
			throw new NotImplementedException();
		}
	}
}
