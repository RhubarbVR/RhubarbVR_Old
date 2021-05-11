using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;

namespace RhubarbEngine.World
{
    public class Worker<T> : IWorldObject
    {
        private World world;

        private IWorldObject parent;

        public RefID referenceID { get; private set; }
        RefID IWorldObject.ReferenceID => referenceID;

        World IWorldObject.World => world;

        IWorldObject IWorldObject.Parent => parent;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => true;

        bool IWorldObject.IsRemoved => false;

        public Worker(World _world, IWorldObject _parent, bool newRefID = true)
        {
            world = _world;
            parent = _parent;
            buildSyncObjs();
            if (newRefID)
            {
                referenceID = _world.buildRefID();
                _world.addWorldObj(this);
            }
        }

        public virtual void buildSyncObjs()
        {

        }

      public virtual DataNodeGroup serialize() {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            DataNodeGroup obj = new DataNodeGroup();
            foreach (var field in fields)
            {
                if (field.GetType() == typeof(Worker<>))
                {
                    obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize());
                }
            }
            DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
            obj.setValue("referenceID", Refid);
            return obj;
        }

        public virtual void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<RefID, RefID> newRefID = default(Dictionary<RefID, RefID>))
        {
            
        }

    }
}
