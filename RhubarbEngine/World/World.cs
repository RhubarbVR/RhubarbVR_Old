using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using System.Reflection;
using RhubarbDataTypes;
using g3;
using RhubarbEngine.Render;
using System.Numerics;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using Org.OpenAPITools.Model;
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
        public UpdateLists updateLists = new UpdateLists();

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


        [NoSync]
        [NoSave]
        public Window grabedWindow;

        [NoSave]
        public SyncUserList users;

        [NoSync]
        [NoSave]
        public User localUser => getLocalUser();
        [NoSync]
        [NoSave]
        private User getLocalUser()
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

        [NoSync]
        [NoSave]
        public User hostUser => getHostUser();
        [NoSync]
        [NoSave]
        private User getHostUser()
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

        public void joinsession(PrivateSession ps,string val)
        {
            waitingForInitSync = true;
            worldObjects.Clear();
            foreach (var item in ps.Sessionconnections)
            {
                if (item != val)
                {
                    netModule.Connect(item);
                }
            }
        }
        private bool waitingForInitSync = false;

        public void LoadSelf()
        {
            user = (byte)users.Count();
            users.Add().LoadFromPrivateUser(worldManager.engine.netApiManager.user);
            userLoaded = true;
        }

        public bool userLoaded = false;

        public void NetworkReceiveEvent(byte[] vale, Peer fromPeer)
        {
            try
            {
                DataNodeGroup dataNodeGroup = new DataNodeGroup(vale);
                if (waitingForInitSync)
                {
                    if (((DataNode<string>)dataNodeGroup.getValue("responses")) != null)
                    {
                        if (((DataNode<string>)dataNodeGroup.getValue("responses")).Value == "WorldSync")
                        {
                            Logger.Log("Loading Start State");
                            try
                            {
                                DataNodeGroup node = ((DataNodeGroup)dataNodeGroup.getValue("data"));
                                List<Action> loadded = new List<Action>();
                                deSerialize(node, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                                LoadSelf();
                                foreach (Action item in loadded)
                                {
                                    item?.Invoke();
                                }
                                waitingForInitSync = false;
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Failed To Load Start State Error" + e.ToString());
                            }

                        }
                    }
                }
                else
                {
                    DataNode<ulong> val = (DataNode<ulong>)dataNodeGroup.getValue("id");
                    DataNode<int> level = (DataNode<int>)dataNodeGroup.getValue("level");
                    try
                    {
                        var member = (ISyncMember)getWorldObj(new NetPointer(val.Value));
                        member.ReceiveData((DataNodeGroup)dataNodeGroup.getValue("data"), fromPeer);
                    }
                    catch
                    {
                        if ((ReliabilityLevel)level.Value != ReliabilityLevel.Unreliable)
                        {
                            if (unassignedValues.ContainsKey(val.Value))
                            {
                                unassignedValues[val.Value].Add(((DataNodeGroup)dataNodeGroup.getValue("data"), DateTime.UtcNow, fromPeer));
                            }
                            else
                            {
                                List<(DataNodeGroup, DateTime, Peer)> saveobjs = new List<(DataNodeGroup, DateTime, Peer)>();
                                saveobjs.Add(((DataNodeGroup)dataNodeGroup.getValue("data"), DateTime.UtcNow, fromPeer));
                                unassignedValues.TryAdd(val.Value, saveobjs);
                            }


                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("Error With Net Reqwest Size: " + vale.Length + " Error: " + e.ToString(), true);
            }
        }

        public void PeerConnectedEvent(Peer peer)
        {
            Logger.Log("We got connection");
            if (!waitingForInitSync)
            {
                Logger.Log("Sent start state");
                DataNodeGroup send = new DataNodeGroup();
                send.setValue("responses", new DataNode<string>("WorldSync"));
                DataNodeGroup value = serialize(true);
                send.setValue("data", value);
                peer.Send(send.getByteArray(), ReliabilityLevel.Reliable);
            }
        }

        public Sync<Vector3f> Gravity;

        public Sync<float> LinearDamping;

        public Sync<float> AngularDamping;

        [NoSaveAttribute]
        public Sync<string> SessionID;

        public DateTime startTime { get; private set; } = DateTime.UtcNow;

        public double worldTime { get { return (startTime - DateTime.UtcNow).TotalSeconds; } }

        public void addDisposable(IDisposable val)
        {
        }
        public void removeDisposable(IDisposable add)
        {
        }
        public Matrix4x4 playerTrans => (userRoot != null)? userRoot.Viewpos : Matrix4x4.CreateScale(1f);
        public Matrix4x4 headTrans => (userRoot != null) ? userRoot.Headpos : Matrix4x4.CreateScale(1f);

        [NoSync]
        [NoSave]
        public UserRoot userRoot => localUser?.userroot.target;

        public void addToRenderQueue(RenderQueue gu, RemderLayers layer)
        {
            switch (focus)
            {
                case FocusLevel.Background:
                    return;
                    break;
                case FocusLevel.Focused:
                    if ((layer & RemderLayers.normal) <= 0) return;
                    break;
                case FocusLevel.Overlay:
                    if ((layer & RemderLayers.overlay) <= 0) return;
                    break;
                case FocusLevel.PrivateOverlay:
                    if ((layer & RemderLayers.privateOverlay) <= 0) return;
                    break;
            }
            Parallel.ForEach(Entitys, ent =>
            {
                if (ent.enabled.value && ent.parentEnabled)
                {
                    ent.addToRenderQueue(gu, playerTrans.Translation, layer);
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

        private FocusLevel focus = FocusLevel.Background;

        public byte posoffset { get; private set; }

        public ulong position = 1;
        public bool userspace { get; private set; }
        public bool local { get; private set; }

        public Entity RootEntity { get; private set; }

        public byte user = 255;

        private Sync<int> _maxUsers;

        public int maxUsers
        {
            get
            {
                if (userspace)
                {
                    return 1;
                }
                else
                {
                    if (_maxUsers.value >= 0)
                    {
                        return _maxUsers.value;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            set
            {
                if (value >= 0)
                {
                    _maxUsers.value = value;
                }
                else
                {
                    _maxUsers.value = 1;
                }
            }
        }
        public DateTime lastFocusChange { get; private set; }

        public WorldManager worldManager;

        private DefaultCollisionConfiguration collisionConfiguration;

        private CollisionDispatcher dispatcher;

        private DbvtBroadphase broadphase;

        public DiscreteDynamicsWorld physicsWorld { get; private set; }

        public FocusLevel Focus
        {
            get
            {
                return focus;
            }
            set
            {
                if (focus != value)
                {
                    focus = value;
                    if (value == FocusLevel.Focused)
                    {
                        if(worldManager.focusedWorld != null)
                        {
                            worldManager.focusedWorld.focus = FocusLevel.Background;
                        }
                        worldManager.focusedWorld = this;
                    }
                    lastFocusChange = DateTime.UtcNow;
                }
            }
        }

        private SynchronizedCollection<Entity> Entitys = new SynchronizedCollection<Entity>();

        private ConcurrentDictionary<NetPointer, IWorldObject> worldObjects = new ConcurrentDictionary<NetPointer, IWorldObject>();

        public ConcurrentDictionary<ulong,List<(DataNodeGroup,DateTime, Peer)>> unassignedValues = new ConcurrentDictionary<ulong, List<(DataNodeGroup, DateTime, Peer)>>();

        public void addWorldObj(IWorldObject obj)
        {
            try
            {
                worldObjects.TryAdd(obj.ReferenceID, obj);
                if (unassignedValues.ContainsKey(obj.ReferenceID.id))
                {
                    try
                    {
                        var member = (ISyncMember)obj;
                        foreach (var item in unassignedValues[obj.ReferenceID.id])
                        {
                            member.ReceiveData((DataNodeGroup)(item.Item1).getValue("data"), (item.Item3));
                        }
                        unassignedValues.TryRemove(obj.ReferenceID.id,out List<(DataNodeGroup, DateTime,Peer)> var);
                    }
                    catch
                    {

                    }
                }

            }
            catch
            {
                worldManager.engine.logger.Log("RefId already existed: "+ obj.ReferenceID.getID().ToString());
            }
        }

        public void addWorldEntity(Entity obj)
        {
            Entitys.SafeAdd(obj);
        }

        public void removeWorldEntity(Entity obj)
        {
            Entitys.Remove(obj);
        }
        [NoSync]
        [NoSave]
        public IWorldObject getWorldObj(NetPointer refid)
        {
            return worldObjects[refid];
        }


        public void removeWorldObj(IWorldObject obj)
        {
            worldObjects.TryRemove(obj.ReferenceID,out IWorldObject value);
            if (!value?.IsRemoved??false)
            {
                value.Dispose();
            }
        }
        NetPointer IWorldObject.ReferenceID => NetPointer.BuildID(1, 0);

        World IWorldObject.World => this;
        [NoSync]
        [NoSave]
        IWorldObject IWorldObject.Parent => null;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => true;
        bool IWorldObject.IsRemoved => false;

        public Sync<string> Name;

        public void Update(DateTime startTime, DateTime Frame)
        {
            physicsWorld.UpdateAabbs();
            physicsWorld.UpdateVehicles(worldManager.engine.platformInfo.deltaSeconds);
            physicsWorld.StepSimulation(worldManager.engine.platformInfo.deltaSeconds);
            physicsWorld.ComputeOverlappingPairs();
            try
            {
                Parallel.ForEach(Entitys, obj =>
                {
                    obj.Update(startTime, Frame);
                });
            }
            catch(Exception e)
            {
            }
            try
            {
                Parallel.ForEach(worldObjects.Values, val =>
                {
                    if (((Worker)val) != null)
                    {
                        ((Worker)val).onUpdate();
                    }
                });
            }
            catch (Exception e)
            {
            }

            netModule.netupdate();
        }


        private void userJoined(User user)
        {
            foreach (IWorldObject val in worldObjects.Values)
            {
                if (((Worker)val) != null)
                {
                    ((Worker)val).onUserJoined(user);
                }
            }
        }

        public NetModule netModule { get; private set; }

        private ConstraintSolverPoolMultiThreaded _constraintSolver;

        public World(WorldManager _worldManager)
        {
            worldManager = _worldManager;
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            broadphase = new DbvtBroadphase();
            if(worldManager.engine.settingsObject.PhysicsSettings.ThreadCount == -1)
            {
                _constraintSolver = new ConstraintSolverPoolMultiThreaded(worldManager.engine.platformInfo.ThreadCount-1);
            }
            else
            {
                _constraintSolver = new ConstraintSolverPoolMultiThreaded(worldManager.engine.settingsObject.PhysicsSettings.ThreadCount);
            }
            physicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, _constraintSolver, collisionConfiguration);
            staticAssets = new StaticAssets(this);
        }


        public World(WorldManager _worldManager, DataNodeGroup node, bool networkload = false):this(_worldManager)
        {
            Random random = new Random();
            posoffset = (byte)random.Next();
            if (posoffset <= 0)
            {
                posoffset = 12;
            }
            SessionID = new Sync<string>(this, this, !networkload);
            Name = new Sync<string>(this, this, !networkload);
            Gravity = new Sync<Vector3f>(this, this, !networkload);
            Gravity.Changed += Gravity_Changed;
            Gravity.value = new Vector3f(0, -10, 0);
            LinearDamping = new Sync<float>(this, this, !networkload);
            AngularDamping = new Sync<float>(this, this, !networkload);
            LinearDamping.value = .03f;
            AngularDamping.value = .03f;
            _maxUsers = new Sync<int>(this, this, !networkload);
            RootEntity = new Entity(this, !networkload);
            users = new SyncUserList(this, !networkload);
            RootEntity.name.value = "Root";
            List<Action> loadded = new List<Action>();
            deSerialize(node, loadded, !networkload, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
            foreach (Action item in loadded)
            {
                item?.Invoke();
            }
        }

        private void Gravity_Changed(IChangeable obj)
        {
            var e = new BulletSharp.Math.Vector3(Gravity.value.x, Gravity.value.y, Gravity.value.z);
            physicsWorld.SetGravity(ref e);
        }

        [NoSave]
        public Sync<SessionsType> sessionsType;
        [NoSave]
        public Sync<AccessLevel> accessLevel;
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
        public World(WorldManager _worldManager, string _Name, int MaxUsers, bool _userspace = false, bool _local = false,DataNodeGroup datanode=null,bool networksession=false) : this(_worldManager)
        {
            Random random = new Random();
            posoffset = (byte)random.Next();
            if(posoffset == 0)
            {
                posoffset = 1;
            }
            SessionID = new Sync<string>(this, this);
            Name = new Sync<string>(this, this);
            Gravity = new Sync<Vector3f>(this, this);
            Gravity.value = new Vector3f(0, -10, 0);
            LinearDamping = new Sync<float>(this, this);
            AngularDamping = new Sync<float>(this, this);
            LinearDamping.value = .03f;
            AngularDamping.value = .03f;
            _maxUsers = new Sync<int>(this, this);
            RootEntity = new Entity(this);
            RootEntity.name.value = "Root";
            Name.value = _Name;
            maxUsers = MaxUsers;
            userspace = _userspace;
            local = _local;
            sessionsType = new Sync<SessionsType>(this, this);
            accessLevel = new Sync<AccessLevel>(this, this);
            Correspondingworlduuid = new Sync<string>(this, this);
            SessionTags = new SyncValueList<string>(this, this);
            Thumbnailurl = new Sync<string>(this, this);
            Eighteenandolder = new Sync<bool>(this, this);
            Mobilefriendly = new Sync<bool>(this, this);
            users = new SyncUserList(this, this);
            sessionsType.value = SessionsType.Casual;
            accessLevel.value = AccessLevel.Anyone;
            if (!userspace && !local)
            {
                netModule = new RhuNetModule(this);
            }
            else
            {
                netModule = new NUllNetModule(this);
              loadHostUser();
            }
            if (datanode != null)
            {
                List<Action> loadded = new List<Action>();
                deSerialize(datanode, loadded, true, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                foreach (Action item in loadded)
                {
                    item?.Invoke();
                }
            }
        }


        public void loadHostUser()
        {
            user = 0;
            waitingForInitSync = false;
            users.Add().LoadFromPrivateUser(worldManager.engine.netApiManager.user);
            userLoaded = true;
            userJoined(hostUser);
        }


        public World(WorldManager _worldManager,SessionsType _sessionsType,AccessLevel _accessLevel, string _Name, int MaxUsers, string worlduuid, bool isOver, bool mobilefriendly,string templet,DataNodeGroup datanode = null) : this( _worldManager,  _Name,  MaxUsers,  false ,  false )
        {
            sessionsType.value = _sessionsType;
            accessLevel.value = _accessLevel;
            Correspondingworlduuid.value = worlduuid;
            Eighteenandolder.value = isOver;
            Mobilefriendly.value = mobilefriendly;
            if(templet != null)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\WorldTemplets\";
                try
                {
                    var data = System.IO.File.ReadAllBytes(path + templet + ".RWorld");
                    DataNodeGroup node = new DataNodeGroup(data);
                    List<Action> loadded = new List<Action>();
                    deSerialize(node, loadded, true, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                    foreach (Action item in loadded)
                    {
                        item?.Invoke();
                    }
                }
                catch(Exception e)
                {

                }
            }
            else
            {
                if(datanode != null)
                {
                    List<Action> loadded = new List<Action>();
                    deSerialize(datanode, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                    foreach (Action item in loadded)
                    {
                        item?.Invoke();
                    }
                }
                else
                {
                    worldManager.BuildLocalWorld(this);
                }
            }
        }

        public NetPointer buildRefID()
        {
            position = position + posoffset;
            if(!worldObjects.ContainsKey(NetPointer.BuildID(position, user)))
            {
                return NetPointer.BuildID(position, user);
            }
            else
            {
                return buildRefID();
            }
        }

        public DataNodeGroup serialize(bool netSave = false)
        {
            FieldInfo[] fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            DataNodeGroup obj = new DataNodeGroup();
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && !(!netSave && (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length > 0)) && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))
                {
                    if (((IWorldObject)field.GetValue(this)) != null)
                    {
                        obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize(netSave));
                    }
                }
            }
            if (netSave)
            {
                obj.setValue("StartTime", new DataNode<DateTime>(startTime));
            }
            return obj;
        }

        public void deSerialize(DataNodeGroup data, List<Action> onload = default(List<Action>), bool NewRefIDs = true, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            FieldInfo[] fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && !(NewRefIDs && (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length > 0)) && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))
                {
                    if (((IWorldObject)field.GetValue(this)) != null)
                    {
                        try
                        {
                            ((IWorldObject)field.GetValue(this)).deSerialize((DataNodeGroup)data.getValue(field.Name), onload, NewRefIDs, newRefID, latterResign);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("Error Deserializeing on world Field name: " + field.Name + " Error:" + e.ToString(), true);
                        }
                    }
                }
            }
            if (NewRefIDs)
            {
                try
                {
                    startTime = ((DataNode<DateTime>)data.getValue("StartTime")).Value;
                }
                catch { }
            }
        }
        public virtual void Dispose()
        {

        }

        public DataNodeGroup serialize()
        {
            return serialize(false);
        }

    }
}
