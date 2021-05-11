using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using System.Reflection;


namespace RhubarbEngine.World
{
    public class World : IWorldObject
    {
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

        private Dictionary<RefID, IWorldObject> worldObjects = new Dictionary<RefID, IWorldObject>();

        public void addWorldObj(IWorldObject obj)
        {
            worldObjects.Add(obj.ReferenceID, obj);
        }

        public void removeWorldObj(IWorldObject obj)
        {
            worldObjects.Remove(obj.ReferenceID);
        }
        RefID IWorldObject.ReferenceID => RefID.BuildID(1, 0);

        World IWorldObject.World => this;

        IWorldObject IWorldObject.Parent => null;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => true;
        bool IWorldObject.IsRemoved => false;

        public Sync<string> Name;

        public void Update(DateTime startTime, DateTime Frame)
        {

        }
        public World(WorldManager _worldManager)
        {
            worldManager = _worldManager;
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

        public RefID buildRefID()
        {
            position = position + posoffset;
            return RefID.BuildID(position, user);
        }

        public DataNodeGroup serialize()
        {
            FieldInfo[] fields = typeof(World).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            DataNodeGroup obj = new DataNodeGroup();
            foreach (var field in fields)
            {
                if (field.GetType() == typeof(Worker<>))
                {
                    obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize());
                }
            }
            return obj;
        }

        public void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>))
        {

        }

    }
}
