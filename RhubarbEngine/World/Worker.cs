using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Managers;
using RhubarbEngine.Render.Material.Fields;

namespace RhubarbEngine.World
{
	public class Worker : IChangeable, IWorldObject
	{
		private List<IDisposable> _disposables = new List<IDisposable>();
		public void addDisposable(IDisposable add)
		{
			try
			{
				_disposables.Add(add);
			}
			catch { }
		}

		public void removeDisposable(IDisposable add)
		{
			try
			{
				_disposables.Remove(add);
			}
			catch { }
		}

		public event Action<IChangeable> Changed;
		[NoShow]
		[NoSync]
		[NoSave]
		public World world { get; protected set; }
		[NoShow]
		[NoSync]
		[NoSave]
		public IWorldObject parent;

		public NetPointer referenceID { get; protected set; }

		public UnitLogs logger => world.worldManager.engine.logger;

		public Engine engine => world.worldManager.engine;

		public InputManager input => engine.inputManager;

		public double DeltaSeconds => world.worldManager.engine.platformInfo.deltaSeconds;

		public bool Persistent = true;

		NetPointer IWorldObject.ReferenceID => referenceID;
		[NoShow]
		[NoSync]
		[NoSave]
		World IWorldObject.World => world;
		[NoShow]
		[NoSync]
		[NoSave]
		IWorldObject IWorldObject.Parent => parent;

		bool IWorldObject.IsLocalObject => false;

		bool IWorldObject.IsPersistent => Persistent;

		bool _Removed = false;

		public bool IsRemoved => _Removed;

		public void Destroy()
		{
			Dispose();
		}

		public Worker(World _world, IWorldObject _parent, bool newRefID = true, bool childlisten = true)
		{
			world = _world;
			parent = _parent;
			parent.addDisposable(this);
			inturnalSyncObjs(newRefID);
			buildSyncObjs(newRefID);
			if (childlisten)
			{
				FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				foreach (var field in fields)
				{
					if (typeof(IChangeable).IsAssignableFrom(field.FieldType) && ((IChangeable)field.GetValue(this)) != null)
					{
						((IChangeable)field.GetValue(this)).Changed += onChangeInternal;

					}
				}
			}
			if (newRefID)
			{
				referenceID = _world.buildRefID();
				_world.addWorldObj(this);
			}
		}

		public virtual void initialize(World _world, IWorldObject _parent, bool newRefID = true)
		{
			world = _world;
			parent = _parent;
			parent.addDisposable(this);
			inturnalSyncObjs(newRefID);
			buildSyncObjs(newRefID);
			FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				if (typeof(IChangeable).IsAssignableFrom(field.FieldType) && ((IChangeable)field.GetValue(this)) != null)
				{
					((IChangeable)field.GetValue(this)).Changed += onChangeInternal;
				}
			}
			if (newRefID)
			{
				referenceID = _world.buildRefID();
				_world.addWorldObj(this);
				onLoaded();
			}
		}
		public Worker()
		{

		}

		public virtual void inturnalSyncObjs(bool newRefIds)
		{

		}


		public virtual void buildSyncObjs(bool newRefIds)
		{

		}

		public virtual void onLoaded()
		{

		}

		public virtual void onUpdate()
		{

		}

		public void onChangeInternal(IChangeable newValue)
		{
			if (Changed != null)
			{
				Changed(this);
			}
			onChanged();
		}
		public virtual void onChanged()
		{

		}
		public virtual void Removed()
		{

		}

		public virtual void onUserJoined(User user)
		{

		}

		public event Action<Worker> onDispose;

		public virtual void Dispose()
		{
			Removed();
			world.removeWorldObj(this);
			onDispose?.Invoke(this);
			foreach (IDisposable dep in _disposables)
			{
				dep.Dispose();
			}
			_Removed = true;
		}
		public virtual void CommonUpdate()
		{

		}

		public virtual DataNodeGroup serialize(bool netsync = false)
		{
			FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			DataNodeGroup obj = null;
			if (Persistent || netsync)
			{
				obj = new DataNodeGroup();
				foreach (var field in fields)
				{
					if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || netsync && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0)))
					{
						//This is for debug purposes 
						//if (!netsync)
						//{
						//    Console.WriteLine(field.FieldType.FullName + "Name: " + field.Name);
						//}
						obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize(netsync));
					}
				}
				DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
				obj.setValue("referenceID", Refid);
			}
			return obj;
		}

		public virtual void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
		{

			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading Node: " + this.GetType().FullName);
				return;
			}
			if (NewRefIDs)
			{
				if (newRefID == null)
				{
					Console.WriteLine("Problem With " + this.GetType().FullName);
				}
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
				if (referenceID.id == new NetPointer(0).id)
				{
					logger.Log(this.GetType().FullName + " RefID null");
				}
				else
				{
					world.addWorldObj(this);
				}
			}

			FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || !NewRefIDs && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0)))
				{
					if (((IWorldObject)field.GetValue(this)) == null)
					{
						throw new Exception("Sync not initialized on " + this.GetType().FullName + " Field: " + field.Name);
					}
					((IWorldObject)field.GetValue(this)).deSerialize((DataNodeGroup)data.getValue(field.Name), onload, NewRefIDs, newRefID, latterResign);
				}
			}
			if (typeof(IRenderObject).IsAssignableFrom(this.GetType()))
			{
				onload.Add(onLoaded);
			}
			else
			{
				onload.Insert(0, onLoaded);
			}
		}

	}
}
