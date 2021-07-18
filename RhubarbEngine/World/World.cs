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
using BepuPhysics;
using BepuUtilities.Memory;
using System.Threading;
using BepuPhysics.Collidables;
using System.Runtime.CompilerServices;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using LiteNetLib;
using LiteNetLib.Utils;
using Org.OpenAPITools.Model;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
    public class World : IWorldObject
    {
        [NoSave]
        public SyncUserList users;

        [NoSync]
        public User localUser => getLocalUser();
        [NoSync]
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
        public User hostUser => getHostUser();
        [NoSync]
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
            foreach (var item in ps.Sessionconnections)
            {
                if (item != val)
                {
                    string ip = item.Split(" _ ")[0];
                    string port = item.Split(" _ ")[1];
                    startClient(ip, Int32.Parse(port));
                }
            }
        }
        private bool waitingForInitSync = false;

        public List<NetData> NetQueue = new List<NetData>();

        public void addToQueue(ReliabilityLevel _reliabilityLevel, DataNodeGroup _data, ulong _id)
        {
            var netdata = new NetData(_reliabilityLevel, _data, _id);
            NetQueue.Add(netdata);
        }

        public EventBasedNetListener listener;

        public EventBasedNetListener ClientListener;

        public LiteNetLib.NetManager server;

        public int port;

        public List<LiteNetLib.NetManager> clients = new List<LiteNetLib.NetManager>();

        public void exsepetConectionReq(ConnectionRequest req)
        {
            req.Accept();
        }

        public void startClient(string ip,int _port)
        {
            LiteNetLib.NetManager client = new LiteNetLib.NetManager(listener);
            client.Start();
            client.Connect(ip, _port, "");
            clients.Add(client);
        }

        public void LoadSelf() 
        {
            user = (byte)users.Count();
            users.Add().LoadFromPrivateUser(worldManager.engine.netApiManager.user);
            userLoaded = true;
        }

        public bool userLoaded = false;

        public void initializeNetWorker()
        {
            listener = new EventBasedNetListener();
            server = new LiteNetLib.NetManager(listener);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                byte[] vale = dataReader.GetBytesWithLength();
                try
                {
                    DataNodeGroup dataNodeGroup = new DataNodeGroup(vale) ;
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
                        try
                        {
                            DataNode<ulong> val = (DataNode<ulong>)dataNodeGroup.getValue("id");
                            var member = (ISyncMember)getWorldObj(new NetPointer(val.Value));
                            member.ReceiveData((DataNodeGroup)dataNodeGroup.getValue("data"), fromPeer);
                        }
                        catch
                        {

                        }
                    }
                }
                catch(Exception e)
                {
                    Logger.Log("Error With Net Reqwest Size: "+ vale.Length + " Error: "+ e.ToString(),true);
                }
                    dataReader.Recycle();
                 
                };

            listener.ConnectionRequestEvent += request =>
            {
                Logger.Log("User Trying Connection");
                exsepetConectionReq(request);

            };
            listener.PeerConnectedEvent += peer =>
            {
                Logger.Log("We got connection");
                if (!waitingForInitSync)
                {
                    Logger.Log("Sent start state");
                    DataNodeGroup send = new DataNodeGroup();
                    send.setValue("responses", new DataNode<string>("WorldSync"));
                    DataNodeGroup value = serialize(true);
                    send.setValue("data", value);
                    var val = new NetDataWriter(true);
                    send.Serialize(val);
                    peer.Send(val, DeliveryMethod.ReliableOrdered);
                }
            };
        }

        private void DropQ()
        {
            List<ulong> trains = new List<ulong>();
            List<NetData> netData = new List<NetData>();
            foreach (var item in NetQueue)
            {
                DataNodeGroup node = new DataNodeGroup();
                node.setValue("data", item.data);
                node.setValue("id", new DataNode<ulong>(item.id));
                if (item.reliabilityLevel == ReliabilityLevel.LatestOnly || item.reliabilityLevel == ReliabilityLevel.Unreliable)
                {
                    if (!trains.Contains(item.id))
                    {
                        trains.Add(item.id);
                        sendData(node, (item.reliabilityLevel == ReliabilityLevel.LatestOnly) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
                    }
                }
                else
                {
                    sendData(node, DeliveryMethod.ReliableOrdered);
                }
                netData.Add(item);
            }
            foreach (var item in netData)
            {
                NetQueue.Remove(item);
            }
        }

        private void sendData(DataNodeGroup data,DeliveryMethod delivery)
        {
            if (!userspace && !local)
            {
                var val = new NetDataWriter(true);
                data.Serialize(val);
                server.SendToAll(val, delivery);
                foreach (var item in clients)
                {
                    item.SendToAll(val, delivery);
                }
            }
        }

        private void startServer()
        {
                server.Start();
                port = server.LocalPort;
        }
        public Sync<Vector3f> Gravity;

        public Sync<float> LinearDamping;

        public Sync<float> AngularDamping;

        [NoSaveAttribute]
        public Sync<string> SessionID;


        public struct DemoPoseIntegratorCallbacks : IPoseIntegratorCallbacks
        {
            private World _world;

            /// <summary>
            /// Gravity to apply to dynamic bodies in the simulation.
            /// </summary>
            public Vector3 Gravity => _world.Gravity.value.ToSystemNumrics();
            /// <summary>
            /// Fraction of dynamic body linear velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.
            /// </summary>
            public float LinearDamping => _world.LinearDamping.value;
            /// <summary>
            /// Fraction of dynamic body angular velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.
            /// </summary>
            public float AngularDamping => _world.AngularDamping.value;

            Vector3 gravityDt;
            float linearDampingDt;
            float angularDampingDt;

            public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

            public void Initialize(Simulation simulation)
            {
                //In this demo, we don't need to initialize anything.
                //If you had a simulation with per body gravity stored in a CollidableProperty<T> or something similar, having the simulation provided in a callback can be helpful.
            }

            /// <summary>
            /// Creates a new set of simple callbacks for the demos.
            /// </summary>
            /// <param name="gravity">Gravity to apply to dynamic bodies in the simulation.</param>
            /// <param name="linearDamping">Fraction of dynamic body linear velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.</param>
            /// <param name="angularDamping">Fraction of dynamic body angular velocity to remove per unit of time. Values range from 0 to 1. 0 is fully undamped, while values very close to 1 will remove most velocity.</param>
            public DemoPoseIntegratorCallbacks(World world) : this()
            {
                _world = world;
            }

            public void PrepareForIntegration(float dt)
            {
                //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
                gravityDt = Gravity * dt;
                //Since these callbacks don't use per-body damping values, we can precalculate everything.
                linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt);
                angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
            {
                //Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
                if (localInertia.InverseMass > 0)
                {
                    velocity.Linear = (velocity.Linear + gravityDt) * linearDampingDt;
                    velocity.Angular = velocity.Angular * angularDampingDt;
                }
                //Implementation sidenote: Why aren't kinematics all bundled together separately from dynamics to avoid this per-body condition?
                //Because kinematics can have a velocity- that is what distinguishes them from a static object. The solver must read velocities of all bodies involved in a constraint.
                //Under ideal conditions, those bodies will be near in memory to increase the chances of a cache hit. If kinematics are separately bundled, the the number of cache
                //misses necessarily increases. Slowing down the solver in order to speed up the pose integrator is a really, really bad trade, especially when the benefit is a few ALU ops.

                //Note that you CAN technically modify the pose in IntegrateVelocity by directly accessing it through the Simulation.Bodies.ActiveSet.Poses, it just requires a little care and isn't directly exposed.
                //If the PositionFirstTimestepper is being used, then the pose integrator has already integrated the pose.
                //If the PositionLastTimestepper or SubsteppingTimestepper are in use, the pose has not yet been integrated.
                //If your pose modification depends on the order of integration, you'll want to take this into account.

                //This is also a handy spot to implement things like position dependent gravity or per-body damping.
            }

        }

        public unsafe struct NoCollisionCallbacks : INarrowPhaseCallbacks
        {
            private World _world;

            public NoCollisionCallbacks(World world)
            {
                _world = world;
            }
            public void Initialize(Simulation simulation)
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
            {
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
            {
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
            {
                return false;
            }

            public void Dispose()
            {
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
            {
                pairMaterial = new PairMaterialProperties();
                return false;
            }
        }

        public Simulation Simulation { get; protected set; }

        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }

        public BufferPool BufferPool { get; private set; }

        public void addDisposable(IDisposable val)
        {

        }
        public Matrix4x4 playerTrans => (userRoot != null)? userRoot.Viewpos : Matrix4x4.CreateScale(1f);

        [NoSync]
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
            foreach(Entity ent in Entitys)
            {
                if (ent.enabled.value && ent.parentEnabled)
                {
                    ent.addToRenderQueue(gu, playerTrans.Translation, layer);
                }
            }
            
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

        public byte user = 0;

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

        private List<Entity> Entitys = new List<Entity>();

        public List<RenderObject> RenderObjects = new List<RenderObject>();

        private Dictionary<NetPointer, IWorldObject> worldObjects = new Dictionary<NetPointer, IWorldObject>();

        public void addWorldObj(IWorldObject obj)
        {
            try
            {
                worldObjects.Add(obj.ReferenceID, obj);
            }
            catch
            {
                worldManager.engine.logger.Log("RefId already existed: "+ obj.ReferenceID.getID().ToString());
            }
        }

        public void addWorldEntity(Entity obj)
        {
            Entitys.Add(obj);
        }

        public void removeWorldEntity(Entity obj)
        {
            Entitys.Remove(obj);
        }

        public IWorldObject getWorldObj(NetPointer refid)
        {
            return worldObjects[refid];
        }


        public void removeWorldObj(IWorldObject obj)
        {
            worldObjects.Remove(obj.ReferenceID);
        }
        NetPointer IWorldObject.ReferenceID => NetPointer.BuildID(1, 0);

        World IWorldObject.World => this;

        IWorldObject IWorldObject.Parent => null;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => true;
        bool IWorldObject.IsRemoved => false;

        public Sync<string> Name;

        public void Update(DateTime startTime, DateTime Frame)
        {
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            try
            {
                foreach (Entity obj in Entitys)
                {
                    obj.Update(startTime, Frame);
                }
            }
            catch(Exception e)
            {
                Logger.Log("Failed To update Entity Error:" + e.ToString(), true);
            }

            foreach (IWorldObject val in worldObjects.Values)
            {
                if (((Worker)val) != null)
                {
                    ((Worker)val).onUpdate();
                }
            }
            netupdate();
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

        private void netupdate()
        {
            if (server != null)
            {
                server.PollEvents();
            }
            foreach (var item in clients)
            {
                item.PollEvents();
            }
            if(NetQueue.Count != 0)
            {
                DropQ();
            }
        }

        public World(WorldManager _worldManager)
        {
            worldManager = _worldManager;
            BufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            Simulation = Simulation.Create(BufferPool, new NoCollisionCallbacks(this), new DemoPoseIntegratorCallbacks(this), new PositionFirstTimestepper());

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
        public World(WorldManager _worldManager, string _Name, int MaxUsers, bool _userspace = false, bool _local = false,DataNodeGroup datanode=null) : this(_worldManager)
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
                initializeNetWorker();
                startServer();
            }
            else
            {
                loadHostUser();
            }
            if (datanode != null)
            {
                List<Action> loadded = new List<Action>();
                deSerialize(datanode, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                foreach (Action item in loadded)
                {
                    item?.Invoke();
                }
            }
        }

        public void loadHostUser()
        {
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
                    deSerialize(node, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
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
                        }catch(Exception e)
                        {
                            Logger.Log("Error Deserializeing on world Field name: " + field.Name + " Error:" + e.ToString(), true);
                        }
                    }
                }
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
