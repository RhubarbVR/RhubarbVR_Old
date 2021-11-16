using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

using BulletSharp;

using RhubarbDataTypes;

using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Helpers;
using RhubarbEngine.Managers;
using RhubarbEngine.Render;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.Net;

using RNumerics;

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

		private bool _waitingForInitSync = false;

		public void LoadSelf()
		{
            users.Add();
            user = (byte)(users.Count() - 1);
            userLoaded = true;
            UserJoined(users[user]);
        }

		public bool userLoaded = false;

		public void NetworkReceiveEvent(byte[] vale, Peer fromPeer, ReliabilityLevel deliveryMethod)
		{
            try
            {
                var dataNodeGroup = new DataNodeGroup(vale);
                try
                {
                    if (_waitingForInitSync)
                    {
                        if (((DataNode<string>)dataNodeGroup.GetValue("responses")) != null)
                        {
                            if (((DataNode<string>)dataNodeGroup.GetValue("responses")).Value == "WorldSync")
                            {
                                worldManager.Engine.Logger.Log("Loading Start State");
                                try
                                {
                                    _worldObjects.Clear();
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
                                    worldManager.Engine.Logger.Log("Failed To Load Start State Error" + e.ToString());
                                }

                            }
                        }
                    }
                    else
                    {
                        var val = (DataNode<ulong>)dataNodeGroup.GetValue("id");
                        try
                        {
                            var member = (ISyncMember)GetWorldObj(new NetPointer(val.Value));
                            member.ReceiveData((DataNodeGroup)dataNodeGroup.GetValue("data"), fromPeer);
                        }
                        catch (Exception e)
                        {
                            worldManager.Engine.Logger.Log("Error With ID"+ val.Value.ToString()+" Net Request Size: " + vale.Length + " Error: " + e.ToString(), true);
                        }
                    }
                }
                catch (Exception e)
                {
                    worldManager.Engine.Logger.Log("Error With Net Request Size: " + vale.Length + " Error: " + e.ToString(), true);
                }
            }
            catch {
                worldManager.Engine.Logger.Log("Error With data Size: " + vale.Length ,true);
            }
        }

		public void PeerConnectedEvent(Peer peer)
		{
            worldManager.Engine.Logger.Log("We got connection");
			if (!_waitingForInitSync && !Starting)
			{
                worldManager.Engine.Logger.Log("Sent start state");
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

        [NoSync]
		[NoSave]
		public Sync<string> MatrixRoomID;

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
					ent.AddToRenderQueue(gu, HeadTrans.Translation, layer, frustum, view);
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

		public IWorldManager worldManager;

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
                    UpdateFocus();
				}
			}
		}

        private void UpdateFocus()
        {
            try
            {
                Parallel.ForEach(_worldObjects, (item) =>
                 {
                     try
                     {
                         if (item.Value.GetType().IsAssignableTo(typeof(IWorker)))
                         {
                             ((IWorker)item.Value)?.OnFocusChange(_focus);
                         }
                     }
                     catch (Exception e)
                     {
                         worldManager.Engine.Logger.Log($"Failed To update focus On {item.Value.GetType()} NetPointer {item.Value.ReferenceID.id} Error: {e}", true);
                     }
                 });
            }
            catch { }
        }

		private readonly SynchronizedCollection<Entity> _entitys = new();

		private readonly ConcurrentDictionary<NetPointer, IWorldObject> _worldObjects = new();

		public void AddWorldObj(IWorldObject obj)
		{
			try
			{
				_worldObjects.TryAdd(obj.ReferenceID, obj);
			}
			catch
			{
				worldManager.Engine.Logger.Log("RefId already existed: " + obj.ReferenceID.GetID().ToString());
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
			PhysicsWorld.UpdateVehicles((float)worldManager.Engine.PlatformInfo.DeltaSeconds);
			PhysicsWorld.StepSimulation((float)worldManager.Engine.PlatformInfo.DeltaSeconds);
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

		public World(IWorldManager _worldManager)
		{
			worldManager = _worldManager;
			_collisionConfiguration = new DefaultCollisionConfiguration();
			_dispatcher = new CollisionDispatcher(_collisionConfiguration);
			_broadphase = new DbvtBroadphase();
			_constraintSolver = worldManager.Engine.SettingsObject.PhysicsSettings.ThreadCount == -1
                ? new ConstraintSolverPoolMultiThreaded(worldManager.Engine.PlatformInfo.ThreadCount - 1)
                : new ConstraintSolverPoolMultiThreaded(worldManager.Engine.SettingsObject.PhysicsSettings.ThreadCount);
            PhysicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _constraintSolver, _collisionConfiguration);
			staticAssets = new StaticAssets(this);
		}


		public World(IWorldManager _worldManager, DataNodeGroup node, bool networkload = false) : this(_worldManager)
		{
			MatrixRoomID = new Sync<string>(this, this, !networkload);
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

        public bool Starting { get; private set; }

        public World(IWorldManager _worldManager, string _Name, int MaxUsers, bool _userspace = false, bool _local = false, string roomID = null, DataNodeGroup datanode = null, bool CreateBlank = false) : this(_worldManager)
		{
            MatrixRoomID = new Sync<string>(this, this)
            {
                Value = roomID
            };
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
			Correspondingworlduuid = new Sync<string>(this, this);
			SessionTags = new SyncValueList<string>(this, this);
			Thumbnailurl = new Sync<string>(this, this);
			Eighteenandolder = new Sync<bool>(this, this);
			Mobilefriendly = new Sync<bool>(this, this);
			users = new SyncUserList(this, this);
            Name.Value = _Name;
            this.MaxUsers = MaxUsers;
            Userspace = _userspace;
            Local = _local;
            if (!Userspace && !Local)
			{
				NetModule = new LNLNetModule(this,roomID);
			}
			else
			{
				NetModule = new NUllNetModule(this);
				LoadHostUser();
                Starting = false;
            }
            worldManager.Engine.Logger.Log("Starting next net module");
            if (!CreateBlank && datanode is not null)
            {
                var loadded = new List<Action>();
                DeSerialize(datanode, loadded, true, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                foreach (var item in loadded)
                {
                    item?.Invoke();
                }
                Starting = false;
            }
            else
            {
                LoadData().ConfigureAwait(false);
            } 
		}
        
        public bool IsStarting
        {
            get
            {
                return _waitingForInitSync||NetModule.IsStarting|| Starting;
            }
        }


        public async Task LoadData()
        {
            bool isNew;
            if(MatrixRoomID.Value is null)
            {
                isNew = false;
            }
            else
            {
                isNew = MatrixRoomID.Value.Length > 5
                    && await worldManager.Engine.NetApiManager.CheckForSession(MatrixRoomID.Value);
            }
            if (isNew)
            {
                worldManager.Engine.Logger.Log("Join Session");
                _waitingForInitSync = true;
            }
            else if (!Userspace && !Local)
            {
                worldManager.Engine.Logger.Log("Building Blank World");
                MeshHelper.BlankWorld(this);
                LoadHostUser();
            }
            Starting = false;
        }

        public void LoadHostUser()
		{
			user = 0;
			_waitingForInitSync = false;
			users.Add();
			userLoaded = true;
			UserJoined(HostUser);
		}

        public NetPointer BuildRefID()
		{
            position++;
            return !_worldObjects.ContainsKey(NetPointer.BuildID(position, user)) ? NetPointer.BuildID(position, user) : BuildRefID();
        }

		public void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = true, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			var fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && !(NewRefIDs && (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length > 0)) && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))
				{
					if (((IWorldObject)field.GetValue(this)) is not null)
					{
                        try
                        {
                            var filedData = (DataNodeGroup)data.GetValue(field.Name);
                            if (filedData is null)
                            {
                                if (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0)
                                {
                                    ((IWorldObject)field.GetValue(this)).DeSerialize(filedData, onload, NewRefIDs, newRefID, latterResign);
                                }
                            }
                            else
                            {
                                ((IWorldObject)field.GetValue(this)).DeSerialize(filedData, onload, NewRefIDs, newRefID, latterResign);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Failed To DeSerialize Failed {field.Name}", e);
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
				catch(Exception _) { }
			}
		}
		public virtual void Dispose()
		{
            var fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType.IsAssignableTo(typeof(IDisposable))&& field.GetValue(this) is not null)
                {
                    try
                    {
                        ((IDisposable)field.GetValue(this)).Dispose();
                    }
                    catch { }
                }
            }
            foreach (var field in fields)
            {
                if (field.FieldType.IsAssignableTo(typeof(IDisposable)) && field.GetValue(this) is not null)
                {
                    try
                    {
                       field.SetValue(this,null);
                    }
                    catch { }
                }
            }
            worldManager.Worlds.Remove(this);
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
