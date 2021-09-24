using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using System.Reflection;
using RhubarbDataTypes;
using RNumerics;
using RhubarbEngine.Render;
using System.Numerics;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.World.Net;
using BulletSharp;
using BulletSharp.Math;
using System.Net;
using RhubarbEngine.Components.Interaction;
using System.Collections.Concurrent;

namespace RhubarbEngine.World
{
	public class World : IWorldObject
	{
		public UpdateLists updateLists = new();

		public StaticAssets staticAssets;

		[NoSync]
		[NoSave]
		public GrabbableHolder lastHolder;



		[NoSync]
		[NoSave]
		public GrabbableHolder LeftLaserGrabbableHolder;

		[NoSync]
		[NoSave]
		public GrabbableHolder RightLaserGrabbableHolder;

		[NoSync]
		[NoSave]
		public GrabbableHolder HeadLaserGrabbableHolder;


		[NoSync]
		[NoSave]
		public GrabbableHolder GetGrabbableHolder(InteractionSource interactionSource)
		{
			switch (interactionSource)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					return LeftLaserGrabbableHolder;
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					return RightLaserGrabbableHolder;
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					return HeadLaserGrabbableHolder;
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
			return null;
		}

		[NoShow]
		[NoSync]
		[NoSave]
		public Window grabedWindow;

		[NoSave]
		public SyncUserList users;

        [NoShow]
        [NoSync]
        [NoSave]
        public User LocalUser
        {
            get
            {
                return GetLocalUser();
            }
        }

        [NoShow]
		[NoSync]
		[NoSave]
		private User GetLocalUser()
		{
			try
			{
				return users[user];
			}
			catch
			{

			}
			return null;
		}
        [NoShow]
        [NoSync]
        [NoSave]
        public User HostUser
        {
            get
            {
                return GetHostUser();
            }
        }

        [NoShow]
		[NoSync]
		[NoSave]
		private User GetHostUser()
		{
			try
			{
				return users[0];
			}
			catch
			{

			}
			return null;
		}
		//public void joinsession(PrivateSession ps, string val)
		//{
		//	waitingForInitSync = true;
		//	worldObjects.Clear();
		//	foreach (var item in ps.Sessionconnections)
		//	{
		//		if (item != val)
		//		{
		//			netModule.Connect(item);
		//		}
		//	}
		//}
		private bool _waitingForInitSync = false;

		public void LoadSelf()
		{
			user = (byte)users.Count();
            users.Add();
			userLoaded = true;
		}

		public bool userLoaded = false;

		public void NetworkReceiveEvent(byte[] vale, Peer fromPeer)
		{
			try
			{
				var dataNodeGroup = new DataNodeGroup(vale);
				if (_waitingForInitSync)
				{
					if (((DataNode<string>)dataNodeGroup.GetValue("responses")) != null)
					{
						if (((DataNode<string>)dataNodeGroup.GetValue("responses")).Value == "WorldSync")
						{
                            worldManager.engine.Logger.Log("Loading Start State");
							try
							{
								var node = (DataNodeGroup)dataNodeGroup.GetValue("data");
								var loadded = new List<Action>();
								DeSerialize(node, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
								LoadSelf();
								foreach (var item in loadded)
								{
									item?.Invoke();
								}
								_waitingForInitSync = false;
							}
							catch (Exception e)
							{
                                worldManager.engine.Logger.Log("Failed To Load Start State Error" + e.ToString());
							}

						}
					}
				}
				else
				{
					var val = (DataNode<ulong>)dataNodeGroup.GetValue("id");
					var level = (DataNode<int>)dataNodeGroup.GetValue("level");
					try
					{
						var member = (ISyncMember)GetWorldObj(new NetPointer(val.Value));
						member.ReceiveData((DataNodeGroup)dataNodeGroup.GetValue("data"), fromPeer);
					}
					catch
					{
						if ((ReliabilityLevel)level.Value != ReliabilityLevel.Unreliable)
						{
							if (unassignedValues.ContainsKey(val.Value))
							{
								unassignedValues[val.Value].Add(((DataNodeGroup)dataNodeGroup.GetValue("data"), DateTime.UtcNow, fromPeer));
							}
							else
							{
                                var saveobjs = new List<(DataNodeGroup, DateTime, Peer)>
                                {
                                    ((DataNodeGroup)dataNodeGroup.GetValue("data"), DateTime.UtcNow, fromPeer)
                                };
                                unassignedValues.TryAdd(val.Value, saveobjs);
							}


						}
					}
				}
			}
			catch (Exception e)
			{
                worldManager.engine.Logger.Log("Error With Net Reqwest Size: " + vale.Length + " Error: " + e.ToString(), true);
			}
		}

		public void PeerConnectedEvent(Peer peer)
		{
            worldManager.engine.Logger.Log("We got connection");
			if (!_waitingForInitSync)
			{
                worldManager.engine.Logger.Log("Sent start state");
				var send = new DataNodeGroup();
				send.SetValue("responses", new DataNode<string>("WorldSync"));
				var value = Serialize(new WorkerSerializerObject(true));
                send.SetValue("data", value);
				peer.Send(send.GetByteArray(), ReliabilityLevel.Reliable);
			}
		}

		public Sync<Vector3f> Gravity;

		public Sync<float> LinearDamping;

		public Sync<float> AngularDamping;

		[NoSaveAttribute]
		public Sync<string> SessionID;

		public DateTime StartTime { get; private set; } = DateTime.UtcNow;

		public double WorldTime { get { return (StartTime - DateTime.UtcNow).TotalSeconds; } }

		public void AddDisposable(IDisposable val)
		{
		}
		public void RemoveDisposable(IDisposable add)
		{
		}
        public Matrix4x4 PlayerTrans
        {
            get
            {
                return (UserRoot != null) ? UserRoot.Viewpos : Matrix4x4.CreateScale(1f);
            }
        }

        public Matrix4x4 HeadTrans
        {
            get
            {
                return (UserRoot != null) ? UserRoot.Headpos : Matrix4x4.CreateScale(1f);
            }
        }

        [NoShow]
        [NoSync]
        [NoSave]
        public UserRoot UserRoot
        {
            get
            {
                return LocalUser?.userroot.Target;
            }
        }

        [NoShow]
		[NoSync]
		[NoSave]
		public RhubarbEngine.Components.ImGUI.EntityObserver lastEntityObserver;

		public void AddToRenderQueue(RenderQueue gu, RemderLayers layer, RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			switch (_focus)
			{
				case FocusLevel.Background:
					return;
				case FocusLevel.Focused:
					if ((layer & RemderLayers.normal) <= 0)
                    {
                        return;
                    }
                    break;
				case FocusLevel.Overlay:
					if ((layer & RemderLayers.overlay) <= 0)
                    {
                        return;
                    }
                    break;
				case FocusLevel.PrivateOverlay:
					if ((layer & RemderLayers.privateOverlay) <= 0)
                    {
                        return;
                    }
                    break;
			}
			Parallel.ForEach(_entitys, ent =>
			{
				if (ent.enabled.Value && ent.parentEnabled)
				{
					ent.AddToRenderQueue(gu, PlayerTrans.Translation, layer, frustum, view);
				}
			});
		}

		public enum FocusLevel
		{
			Background,
			Focused,
			Overlay,
			PrivateOverlay
		}

		private FocusLevel _focus = FocusLevel.Background;

		public byte Posoffset { get; private set; }

		public ulong position = 1;
		public bool Userspace { get; private set; }
		public bool Local { get; private set; }

		public Entity RootEntity { get; private set; }

		public byte user = 255;

		private readonly Sync<int> _maxUsers;

		public int MaxUsers
		{
			get
			{
                return Userspace ? 1 : _maxUsers.Value >= 0 ? _maxUsers.Value : 1;
            }
			set
			{
				_maxUsers.Value = value >= 0 ? value : 1;
            }
		}
		public DateTime LastFocusChange { get; private set; }

		public WorldManager worldManager;

		private readonly DefaultCollisionConfiguration _collisionConfiguration;

		private readonly CollisionDispatcher _dispatcher;

		private readonly DbvtBroadphase _broadphase;

		public DiscreteDynamicsWorld PhysicsWorld { get; private set; }

		public FocusLevel Focus
		{
			get
			{
				return _focus;
			}
			set
			{
				if (_focus != value)
				{
					_focus = value;
					if (value == FocusLevel.Focused)
					{
						if (worldManager.FocusedWorld != null)
						{
							worldManager.FocusedWorld._focus = FocusLevel.Background;
						}
						worldManager.FocusedWorld = this;
					}
					LastFocusChange = DateTime.UtcNow;
				}
			}
		}

		private readonly SynchronizedCollection<Entity> _entitys = new();

		private readonly ConcurrentDictionary<NetPointer, IWorldObject> _worldObjects = new();

		public ConcurrentDictionary<ulong, List<(DataNodeGroup, DateTime, Peer)>> unassignedValues = new();

		public void AddWorldObj(IWorldObject obj)
		{
			try
			{
				_worldObjects.TryAdd(obj.ReferenceID, obj);
				if (unassignedValues.ContainsKey(obj.ReferenceID.id))
				{
					try
					{
						var member = (ISyncMember)obj;
						foreach (var item in unassignedValues[obj.ReferenceID.id])
						{
							member.ReceiveData((DataNodeGroup)item.Item1.GetValue("data"), item.Item3);
						}
						unassignedValues.TryRemove(obj.ReferenceID.id, out var var);
					}
					catch
					{

					}
				}

			}
			catch
			{
				worldManager.engine.Logger.Log("RefId already existed: " + obj.ReferenceID.getID().ToString());
			}
		}

		public void AddWorldEntity(Entity obj)
		{
			_entitys.SafeAdd(obj);
		}

		public void RemoveWorldEntity(Entity obj)
		{
			_entitys.Remove(obj);
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public IWorldObject GetWorldObj(NetPointer refid)
		{
			return _worldObjects[refid];
		}


		public void RemoveWorldObj(IWorldObject obj)
		{
			_worldObjects.TryRemove(obj.ReferenceID, out var value);
			if (!value?.IsRemoved ?? false)
			{
				value.Dispose();
			}
		}
        NetPointer IWorldObject.ReferenceID
        {
            get
            {
                return NetPointer.BuildID(1, 0);
            }
        }

        World IWorldObject.World
        {
            get
            {
                return this;
            }
        }

        [NoShow]
        [NoSync]
        [NoSave]
        IWorldObject IWorldObject.Parent
        {
            get
            {
                return null;
            }
        }

        bool IWorldObject.IsLocalObject
        {
            get
            {
                return false;
            }
        }

        bool IWorldObject.IsPersistent
        {
            get
            {
                return true;
            }
        }

        bool IWorldObject.IsRemoved
        {
            get
            {
                return false;
            }
        }

        public Sync<string> Name;

		public void Update(DateTime startTime, DateTime Frame)
		{
			PhysicsWorld.UpdateAabbs();
			PhysicsWorld.UpdateVehicles(worldManager.engine.PlatformInfo.DeltaSeconds);
			PhysicsWorld.StepSimulation(worldManager.engine.PlatformInfo.DeltaSeconds);
			PhysicsWorld.ComputeOverlappingPairs();
			try
			{
				Parallel.ForEach(_entitys, obj => obj.Update(startTime, Frame));
			}
			catch
			{
			}
			try
			{
				Parallel.ForEach(_worldObjects.Values, val =>
				{
					if (((IWorker)val) != null)
					{
						((IWorker)val).OnUpdate();
					}
				});
			}
			catch
			{
			}

			NetModule.Netupdate();
		}


		private void UserJoined(User user)
		{
			foreach (var val in _worldObjects.Values)
			{
				if (((IWorker)val) != null)
				{
					((IWorker)val).OnUserJoined(user);
				}
			}
		}

		public NetModule NetModule { get; private set; }

		private readonly ConstraintSolverPoolMultiThreaded _constraintSolver;

		public World(WorldManager _worldManager)
		{
			worldManager = _worldManager;
			_collisionConfiguration = new DefaultCollisionConfiguration();
			_dispatcher = new CollisionDispatcher(_collisionConfiguration);
			_broadphase = new DbvtBroadphase();
			_constraintSolver = worldManager.engine.SettingsObject.PhysicsSettings.ThreadCount == -1
                ? new ConstraintSolverPoolMultiThreaded(worldManager.engine.PlatformInfo.ThreadCount - 1)
                : new ConstraintSolverPoolMultiThreaded(worldManager.engine.SettingsObject.PhysicsSettings.ThreadCount);
            PhysicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _constraintSolver, _collisionConfiguration);
			staticAssets = new StaticAssets(this);
		}


		public World(WorldManager _worldManager, DataNodeGroup node, bool networkload = false) : this(_worldManager)
		{
			var random = new Random();
			Posoffset = (byte)random.Next();
			if (Posoffset <= 0)
			{
				Posoffset = 12;
			}
			SessionID = new Sync<string>(this, this, !networkload);
			Name = new Sync<string>(this, this, !networkload);
			Gravity = new Sync<Vector3f>(this, this, !networkload);
			Gravity.Changed += Gravity_Changed;
			Gravity.Value = new Vector3f(0, -10, 0);
			LinearDamping = new Sync<float>(this, this, !networkload);
			AngularDamping = new Sync<float>(this, this, !networkload);
			LinearDamping.Value = .03f;
			AngularDamping.Value = .03f;
			_maxUsers = new Sync<int>(this, this, !networkload);
			RootEntity = new Entity(this, !networkload);
			users = new SyncUserList(this, !networkload);
			RootEntity.name.Value = "Root";
			var loadded = new List<Action>();
			DeSerialize(node, loadded, !networkload, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
			foreach (var item in loadded)
			{
				item?.Invoke();
			}
		}

		private void Gravity_Changed(IChangeable obj)
		{
			var e = new BulletSharp.Math.Vector3(Gravity.Value.x, Gravity.Value.y, Gravity.Value.z);
			PhysicsWorld.SetGravity(ref e);
		}

		[NoSave]
		public Sync<string> Correspondingworlduuid;
		[NoSave]
		public SyncValueList<string> SessionTags;
		[NoSave]
		public Sync<string> Thumbnailurl;
		[NoSave]
		public Sync<bool> Eighteenandolder;
		[NoSave]
		public Sync<bool> Mobilefriendly;
		public World(WorldManager _worldManager, string _Name, int MaxUsers, bool _userspace = false, bool _local = false, DataNodeGroup datanode = null) : this(_worldManager)
		{
			var random = new Random();
			Posoffset = (byte)random.Next();
			if (Posoffset == 0)
			{
				Posoffset = 1;
			}
			SessionID = new Sync<string>(this, this);
			Name = new Sync<string>(this, this);
            Gravity = new Sync<Vector3f>(this, this)
            {
                Value = new Vector3f(0, -10, 0)
            };
            LinearDamping = new Sync<float>(this, this);
			AngularDamping = new Sync<float>(this, this);
			LinearDamping.Value = .03f;
			AngularDamping.Value = .03f;
			_maxUsers = new Sync<int>(this, this);
			RootEntity = new Entity(this);
			RootEntity.name.Value = "Root";
			Name.Value = _Name;
			this.MaxUsers = MaxUsers;
			Userspace = _userspace;
			Local = _local;
			Correspondingworlduuid = new Sync<string>(this, this);
			SessionTags = new SyncValueList<string>(this, this);
			Thumbnailurl = new Sync<string>(this, this);
			Eighteenandolder = new Sync<bool>(this, this);
			Mobilefriendly = new Sync<bool>(this, this);
			users = new SyncUserList(this, this);
			if (!Userspace && !Local)
			{
				NetModule = new RhuNetModule(this);
			}
			else
			{
				NetModule = new NUllNetModule(this);
				LoadHostUser();
			}
			if (datanode != null)
			{
				var loadded = new List<Action>();
				DeSerialize(datanode, loadded, true, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
				foreach (var item in loadded)
				{
					item?.Invoke();
				}
			}
		}


		public void LoadHostUser()
		{
			user = 0;
			_waitingForInitSync = false;
			users.Add();
			userLoaded = true;
			UserJoined(HostUser);
		}


		public World(WorldManager _worldManager, string _Name, int MaxUsers, string worlduuid, bool isOver, bool mobilefriendly, string templet, DataNodeGroup datanode = null) : this(_worldManager, _Name, MaxUsers, false, false)
		{
			Correspondingworlduuid.Value = worlduuid;
			Eighteenandolder.Value = isOver;
			Mobilefriendly.Value = mobilefriendly;
			if (templet != null)
			{
				var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorldTemplets");
				try
				{
					var data = System.IO.File.ReadAllBytes(Path.Combine(path, templet + ".RWorld"));
					var node = new DataNodeGroup(data);
					var loadded = new List<Action>();
					DeSerialize(node, loadded, true, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
					foreach (var item in loadded)
					{
						item?.Invoke();
					}
				}
				catch
				{

				}
			}
			else
			{
				if (datanode != null)
				{
					var loadded = new List<Action>();
					DeSerialize(datanode, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
					foreach (var item in loadded)
					{
						item?.Invoke();
					}
				}
				else
				{
                    WorldManager.BuildLocalWorld(this);
				}
			}
		}

		public NetPointer BuildRefID()
		{
			position += Posoffset;
            return !_worldObjects.ContainsKey(NetPointer.BuildID(position, user)) ? NetPointer.BuildID(position, user) : BuildRefID();
        }

		public void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = true, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			var fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && !(NewRefIDs && (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length > 0)) && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))
				{
					if (((IWorldObject)field.GetValue(this)) != null)
					{
						try
						{
							((IWorldObject)field.GetValue(this)).DeSerialize((DataNodeGroup)data.GetValue(field.Name), onload, NewRefIDs, newRefID, latterResign);
						}
						catch (Exception e)
						{
                            worldManager.engine.Logger.Log("Error Deserializeing on world Field name: " + field.Name + " Error:" + e.ToString(), true);
						}
					}
				}
			}
			if (NewRefIDs)
			{
				try
				{
					StartTime = ((DataNode<DateTime>)data.GetValue("StartTime")).Value;
				}
				catch { }
			}
		}
		public virtual void Dispose()
		{

		}

		public DataNodeGroup Serialize()
		{
			return Serialize(new WorkerSerializerObject(false));
		}

        public DataNodeGroup Serialize(WorkerSerializerObject serializerObject)
        {
            return serializerObject.CommonWorkerSerialize(this);
        }
    }
}
