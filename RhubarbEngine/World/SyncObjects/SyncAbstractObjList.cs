using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.Net;
using System.Collections;

namespace RhubarbEngine.World
{
	public class SyncAbstractObjList<T> : Worker, ISyncList, IWorldObject, ISyncMember where T : IWorker
	{
        public SyncAbstractObjList() { }
        private readonly SynchronizedCollection<T> _synclist = new(25);

		public T this[int i]
		{
			get
			{
				return _synclist[i];
			}
		}

		public IEnumerator<T> GetEnumerator()
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
		public T Add(T val, bool Refid = true)
        {
            val.Initialize(World, this, Refid);
            if (!Refid)
            {
                val.OnLoaded();
            }
            AddInternal(val);
			if (Refid)
			{
				NetAdd(val);
			}
			return _synclist[_synclist.Count - 1];
		}
		public T Add<L>(bool Refid = true) where L : T
		{
			var val = (L)Activator.CreateInstance(typeof(L));
			val.Initialize(World, this, Refid);
            if (!Refid)
            {
                val.OnLoaded();
            }
            AddInternal(val);
			if (Refid)
			{
				NetAdd(val);
			}
			return _synclist[_synclist.Count - 1];
		}

		public void AddInternal(T value)
		{
			_synclist.SafeAdd(value);
			value.OnDispose += Value_onDispose;
			AddDisposable(value);
		}

		public void RemoveInternal(T value)
		{
			_synclist.Remove(value);
			value.OnDispose -= Value_onDispose;
			RemoveDisposable(value);
		}

		private void Value_onDispose(IWorker worker)
		{
			try
			{
				_synclist.Remove((T)worker);
			}
			catch
			{

			}
		}

		public T Add(Type type, bool Refid = true)
		{
			var val = (T)Activator.CreateInstance(type);
			val.Initialize(World, this, Refid);
            val.OnLoaded();
            AddInternal(val);
			if (Refid)
			{
				NetAdd(val);
			}
			return _synclist[_synclist.Count - 1];
		}

		private void NetAdd(T val)
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(0));
			var tip = val.Serialize(new WorkerSerializerObject(true));
            var listobj = new DataNodeGroup();
			if (tip != null)
			{
				listobj.SetValue("Value", tip);
			}
			//Need To add Constant Type Strings for better compression 
			listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
			send.SetValue("Data", listobj);
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, ReferenceID.id);
		}

		private void NetClear()
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(1));
			World.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, ReferenceID.id);
		}

		public void Clear()
		{
			_synclist.Clear();
			NetClear();
		}

		public void ReceiveData(DataNodeGroup data, Peer peer)
		{
			try
			{
				if (((DataNode<byte>)data.GetValue("Type")).Value == 1)
				{
					_synclist.Clear();
				}
				else
				{
					var ty = Type.GetType(((DataNode<string>)((DataNodeGroup)data.GetValue("Data")).GetValue("Type")).Value);
					if (ty == null)
					{
						Logger.Log("Type not found" + ((DataNode<string>)((DataNodeGroup)data.GetValue("Data")).GetValue("Type")).Value, true);
					}
					else
					{

						var val = (T)Activator.CreateInstance(ty);
                        Add(val, false);
						var actions = new List<Action>();
						val.DeSerialize((DataNodeGroup)((DataNodeGroup)data.GetValue("Data")).GetValue("Value"), actions, false);
						foreach (var item in actions)
						{
							item?.Invoke();
						}
                        val.OnLoaded();
                    }
				}
			}
			catch (Exception e)
			{
				Logger.Log("Error With net sync ab e:" + e.ToString());

			}

		}

		public SyncAbstractObjList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}

		public SyncAbstractObjList(IWorldObject _parent, bool refid = true) : base(_parent.World, _parent, refid)
		{

		}

		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
			return workerSerializerObject.CommonListAbstactSerialize<T>(this,_synclist);
		}
		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
                throw new Exception("Node did not exsets When loading SyncAbstractObjList");
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
				var ty = Type.GetType(((DataNode<string>)val.GetValue("Type")).Value);
				if (ty == typeof(MissingComponent))
				{
					ty = Type.GetType(((DataNode<string>)((DataNodeGroup)val.GetValue("Value")).GetValue("type")).Value, false);
					if (ty == null)
					{
						World.worldManager.Engine.Logger.Log("Component still not found" + ((DataNode<string>)val.GetValue("Type")).Value);
						var obj = (T)Activator.CreateInstance(typeof(MissingComponent));
						Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)val.GetValue("Value"), onload, NewRefIDs, newRefID, latterResign);
					}
					else if (ty != typeof(MissingComponent))
					{
						if (ty.IsAssignableFrom(typeof(T)))
						{
							var obj = (T)Activator.CreateInstance(ty);
							Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)((DataNodeGroup)val.GetValue("Value")).GetValue("Data"), onload, NewRefIDs, newRefID, latterResign);
						}
						else
						{
							World.worldManager.Engine.Logger.Log("Something is broken or someone is messing with things", true);
						}
                    }
                    else
                    {
                        var obj = (T)Activator.CreateInstance(ty);
                        Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)val.GetValue("Value"), onload, NewRefIDs, newRefID, latterResign);
                    }
				}
				else
				{
					if (ty == null)
					{
						World.worldManager.Engine.Logger.Log("Type not found" + ((DataNode<string>)val.GetValue("Type")).Value, true);
						if (typeof(T) == typeof(Component))
						{
							var obj = (T)Activator.CreateInstance(typeof(MissingComponent));
							Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)val.GetValue("Value"), onload, NewRefIDs, newRefID, latterResign);
						}
					}
					else
					{
						var obj = (T)Activator.CreateInstance(ty);
						Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)val.GetValue("Value"), onload, NewRefIDs, newRefID, latterResign);
					}
				}
			}
		}

		IEnumerator<IWorldObject> IEnumerable<IWorldObject>.GetEnumerator()
		{
			for (var i = 0; i < _synclist.Count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IWorldObject>)this).GetEnumerator();
		}

		public bool TryToAddToSyncList()
		{
			throw new NotSupportedException();
		}

		public void Remove(int index)
		{
			throw new NotImplementedException();
		}
	}
}
