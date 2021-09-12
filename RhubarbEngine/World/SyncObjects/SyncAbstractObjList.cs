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
	public class SyncAbstractObjList<T> : Worker, ISyncList, IWorldObject, ISyncMember where T : Worker
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
			val.initialize(world, this, Refid);
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
			val.initialize(world, this, Refid);
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
			value.onDispose += Value_onDispose;
			AddDisposable(value);
		}

		public void RemoveInternal(T value)
		{
			_synclist.Remove(value);
			value.onDispose -= Value_onDispose;
			RemoveDisposable(value);
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

		public T Add(Type type, bool Refid = true)
		{
			var val = (T)Activator.CreateInstance(type);
			val.initialize(world, this, Refid);
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
			var tip = val.Serialize(true);
            var listobj = new DataNodeGroup();
			if (tip != null)
			{
				listobj.SetValue("Value", tip);
			}
			//Need To add Constant Type Strings for better compression 
			listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
			send.SetValue("Data", listobj);
			world.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
		}

		private void NetClear()
		{
			var send = new DataNodeGroup();
			send.SetValue("Type", new DataNode<byte>(1));
			world.NetModule?.AddToQueue(Net.ReliabilityLevel.Reliable, send, referenceID.id);
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
						logger.Log("Type not found" + ((DataNode<string>)((DataNodeGroup)data.GetValue("Data")).GetValue("Type")).Value, true);
					}
					else
					{
						var val = (T)Activator.CreateInstance(ty);
						val.initialize(world, this, false);
						var actions = new List<Action>();
						val.DeSerialize((DataNodeGroup)((DataNodeGroup)data.GetValue("Data")).GetValue("Value"), actions, false);
						_synclist.SafeAdd(val);
						foreach (var item in actions)
						{
							item?.Invoke();
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Log("Error With net sync ab e:" + e.ToString());

			}

		}

		public SyncAbstractObjList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}

		public SyncAbstractObjList(IWorldObject _parent, bool refid = true) : base(_parent.World, _parent, refid)
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
				var listobj = new DataNodeGroup();
				if (tip != null)
				{
					listobj.SetValue("Value", tip);
				}
				//Need To add Constant Type Strings for better compression 
				listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
				list.Add(listobj);
			}
			obj.SetValue("list", list);
			return obj;
		}
		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading SyncAbstractObjList");
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
			foreach (DataNodeGroup val in ((DataNodeList)data.GetValue("list")))
			{
				var ty = Type.GetType(((DataNode<string>)val.GetValue("Type")).Value);
				if (ty == typeof(MissingComponent))
				{
					ty = Type.GetType(((DataNode<string>)((DataNodeGroup)val.GetValue("Value")).GetValue("type")).Value, true);
					if (ty == null)
					{
						world.worldManager.engine.logger.Log("Component still not found" + ((DataNode<string>)val.GetValue("Type")).Value);
						var obj = (T)Activator.CreateInstance(typeof(MissingComponent));
						Add(obj, NewRefIDs).DeSerialize((DataNodeGroup)val.GetValue("Value"), onload, NewRefIDs, newRefID, latterResign);
					}
					else
					{
						if ((ty).IsAssignableFrom(typeof(T)))
						{
							var obj = (T)Activator.CreateInstance(ty);
							Add(obj, NewRefIDs).DeSerialize(((DataNodeGroup)((DataNodeGroup)val.GetValue("Value")).GetValue("Data")), onload, NewRefIDs, newRefID, latterResign);
						}
						else
						{
							world.worldManager.engine.logger.Log("Something is broken or someone is messing with things", true);
						}
					}
				}
				else
				{
					if (ty == null)
					{
						world.worldManager.engine.logger.Log("Type not found" + ((DataNode<string>)val.GetValue("Type")).Value, true);
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
