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
    public class SyncObjList<T> : Worker,IReadOnlyList<IWorldObject>, ISyncList, IWorldObject, ISyncMember where T : IWorker, new()
    {
        public SyncObjList() { }
        private readonly SynchronizedCollection<T> _synclist = new(25);

        int IReadOnlyCollection<IWorldObject>.Count
        {
            get
            {
                return Count();
            }
        }

        IWorldObject IReadOnlyList<IWorldObject>.this[int index]
        {
            get
            {
                return _synclist[index];
            }
        }

        public event Action<IWorker,int> ElementRemoved;
        public event Action<IWorker> ElementAdded;
        public event Action ClearElements;

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
			value.OnDispose += Value_onDispose;
			AddDisposable(value);
            ElementAdded?.Invoke(value);
        }

		public int GetIndexOf(T val)
		{
			return _synclist.IndexOf(val);
		}

		private void Value_onDispose(IWorker worker)
		{
			try
            {
                var index = _synclist.IndexOf((T)worker);
                _synclist.Remove((T)worker);
                ElementRemoved?.Invoke((T)worker, index);
            }
            catch
			{

			}
		}

		public void RemoveInternal(T value)
		{
            var index = _synclist.IndexOf(value);
            _synclist.Remove(value);
			value.OnDispose -= Value_onDispose;
			RemoveDisposable(value);
            ElementRemoved?.Invoke(value, index);
        }

		public T Add(bool Refid = true)
		{
			var a = new T();
			a.Initialize(World, this, Refid);
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
            ClearElements?.Invoke();
        }

        private void NetAdd(T val)
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
                ClearElements?.Invoke();
            }
            else
			{
                var a = Add(false);
				var actions = new List<Action>();
				a.DeSerialize((DataNodeGroup)data.GetValue("Data"), actions, false);
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
		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
            return workerSerializerObject.CommonListSerialize(this,_synclist.Cast<IWorldObject>());
		}
		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
                throw new Exception("Node did not exsets When loading SyncObjList");
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
            _synclist[index].Dispose();
		}
	}
}
