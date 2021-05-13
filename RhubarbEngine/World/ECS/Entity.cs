using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseR;

namespace RhubarbEngine.World.ECS
{
    public class Entity: Worker
    {
        public Sync<Vector3> position;

        public Sync<Quaternion> rotation;

        public Sync<Vector3> scale;

        public SyncRef<Entity> parent;

        public Sync<string> name;

        public Sync<bool> enabled;

        private SyncObjList<Entity> _children;

        private SyncAbstractObjList<Component> _components;

        public override void inturnalSyncObjs(bool newRefIds)
        {
            world.addWorldEntity(this);
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            position = new Sync<Vector3>(this, newRefIds);
            scale = new Sync<Vector3>(this, newRefIds);
            scale.value = Vector3.One;
            rotation = new Sync<Quaternion>(this, newRefIds);
            name = new Sync<string>(this, newRefIds);
            enabled = new Sync<bool>(this, newRefIds);
            _children = new SyncObjList<Entity>(this, newRefIds);
            _components = new SyncAbstractObjList<Component>(this, newRefIds);
            enabled.value = true;
            parent = new SyncRef<Entity>(this, newRefIds);
        }

        public Entity addChild(string name = "Entity")
        {
            Entity val = _children.Add(true);
            val.parent.target = this;
            val.name.value = name;
            return val;
        }

        public T attachComponent<T>() where T: Component
        {
            T newcomp = (T)Activator.CreateInstance(typeof(T));
            _components.Add(newcomp);
            return newcomp;
        }
        public override void onLoaded()
        {

        }

        public override void Dispose()
        {
            world.removeWorldObj(this);
            world.removeWorldEntity(this);
        }

        public void Update(DateTime startTime, DateTime Frame)
        {
            foreach (Component comp in _components)
            {
                comp.CommonUpdate( startTime, Frame);
            }
        }

        public Entity(IWorldObject _parent,bool newRefIds=true) : base(_parent.World, _parent, newRefIds)
        {

        }
        public Entity()
        {
        }
    }
}
