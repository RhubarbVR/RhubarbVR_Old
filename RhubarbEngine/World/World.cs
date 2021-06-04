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

namespace RhubarbEngine.World
{
    public class World : IWorldObject
    {
        public void addDisposable(IDisposable val)
        {

        }
        public Matrix4x4 playerTrans => (userRoot != null)? userRoot.entity.globalTrans() : Matrix4x4.CreateScale(1f);

        [NoSaveAttribute]
        public UserRoot userRoot;

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
            foreach(Entity ent in RootEntity._children)
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
            if(userRoot == null)
            {
                Entity rootent = RootEntity.addChild();
                userRoot = rootent.attachComponent<UserRoot>();
            }
            foreach (Entity obj in Entitys)
            {
                obj.Update(startTime, Frame);
            }
            foreach (IWorldObject val in worldObjects.Values)
            {
                if (((Worker)val) != null)
                {
                    ((Worker)val).onUpdate();
                }
            }
        }
        public World(WorldManager _worldManager)
        {
            worldManager = _worldManager;

        }

        public World(WorldManager _worldManager, DataNodeGroup node, bool networkload = false)
        {
            worldManager = _worldManager;
            Random random = new Random();
            posoffset = (byte)random.Next();
            if (posoffset <= 0)
            {
                posoffset = 12;
            }
            Name = new Sync<string>(this, this, !networkload);
            _maxUsers = new Sync<int>(this, this, !networkload);
            RootEntity = new Entity(this, !networkload);
            RootEntity.name.value = "Root";
            deSerialize(node, !networkload  , new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
        }

        public World(WorldManager _worldManager, string _Name, int MaxUsers, bool _userspace = false)
        {
            worldManager = _worldManager;
            Random random = new Random();
            posoffset = (byte)random.Next();
            Name = new Sync<string>(this, this);
            _maxUsers = new Sync<int>(this, this);
            RootEntity = new Entity(this);
            RootEntity.name.value = "Root";
            Name.value = _Name;
            maxUsers = MaxUsers;
            userspace = _userspace;
        }

        public NetPointer buildRefID()
        {
            position = position + posoffset;
            return NetPointer.BuildID(position, user);
        }

        public DataNodeGroup serialize()
        {
            FieldInfo[] fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            DataNodeGroup obj = new DataNodeGroup();
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0))
                {
                    if (((IWorldObject)field.GetValue(this)) != null)
                    {
                        obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize());
                    }
                }
            }
            return obj;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = true, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>))
        {
            FieldInfo[] fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType)&& (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0))
                {
                    if (((IWorldObject)field.GetValue(this)) != null)
                    {
                        ((IWorldObject)field.GetValue(this)).deSerialize((DataNodeGroup)data.getValue(field.Name), NewRefIDs, newRefID, latterResign);
                    }
                }
            }
        }
        public virtual void Dispose()
        {

        }




    }
}
