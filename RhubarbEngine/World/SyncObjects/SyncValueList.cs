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
	public class SyncValueList<T> : Worker, ISyncList, IWorldObject, ISyncMember where T : IConvertible
	{
		private SynchronizedCollection<Sync<T>> _synclist = new SynchronizedCollection<Sync<T>>(5);

		public int Count => _synclist.Count;

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < _synclist.Count; i++)
			{
				yield return this[i].value;
			}
		}
		public IEnumerator<Sync<T>> GetSyncEnumerator()
		{
			for (int i = 0; i < _synclist.Count; i++)
			{
				yield return this[i];
			}
		}
		public Sync<T> this[int i]
		{
			get
			{
				return _synclist[i];
			}
		}

		public Sync<T> Add(bool Refid = true)
		{
			var val = new Sync<T>(this.world, this, Refid);
			_synclist.SafeAdd(val);
			val.Changed += Val_Changed;
			if (Refid)
			{
				netAdd(val);
			}
			return val;
		}


		private void netAdd(Sync<T> val)
		{
			DataNodeGroup send = new DataNodeGroup();
			send.setValue("Type", new DataNode<byte>(0));
			DataNodeGroup tip = val.serialize(true);
			send.setValue("Data", tip);
			world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
		}

		private void netClear()
		{
			DataNodeGroup send = new DataNodeGroup();
			send.setValue("Type", new DataNode<byte>(1));
			world.netModule?.addToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
		}


		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			if (((DataNode<byte>)data.getValue("Type")).Value == 1)
			{
				_synclist.Clear();
			}
			else
			{
				Sync<T> a = new Sync<T>(this, false);
				List<Action> actions = new List<Action>();
				a.Changed += Val_Changed;
				a.deSerialize((DataNodeGroup)data.getValue("Value"), actions, false);
				foreach (var item in actions)
				{
					item?.Invoke();
				}
				_synclist.SafeAdd(a);
			}
		}


		private void Val_Changed(IChangeable obj)
		{
			onChangeInternal(obj);
		}

		public void Clear()
		{
			_synclist.Clear();
			netClear();
		}
		public SyncValueList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public SyncValueList(IWorldObject _parent, bool newRefID) : base(_parent.World, _parent, newRefID)
		{

		}
		public override DataNodeGroup serialize(bool netsync = false)
		{
			DataNodeGroup obj = new DataNodeGroup();
			DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
			obj.setValue("referenceID", Refid);
			DataNodeList list = new DataNodeList();
			foreach (Sync<T> val in _synclist)
			{
				DataNodeGroup tip = val.serialize(netsync);
				if (tip != null)
				{
					list.Add(tip);
				}
			}
			obj.setValue("list", list);
			return obj;
		}
		public override void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
		{
			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading SyncRef");
				return;
			}
			if (NewRefIDs)
			{
				newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID(), referenceID.getID());
				if (latterResign.ContainsKey(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()))
				{
					foreach (RefIDResign func in latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()])
					{
						func(referenceID.getID());
					}
				}
			}
			else
			{
				referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
				world.addWorldObj(this);
			}
			foreach (DataNodeGroup val in ((DataNodeList)data.getValue("list")))
			{
				Add(NewRefIDs).deSerialize(val, onload, NewRefIDs, newRefID, latterResign);
			}
		}

		int ISyncList.Count()
		{
			return this.Count();
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
