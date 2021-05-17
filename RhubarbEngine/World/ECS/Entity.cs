using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.Render;

namespace RhubarbEngine.World.ECS
{
    public class Entity: Worker
    {
        public Sync<Vector3f> position;

        public Sync<Quaternionf> rotation;

        public Sync<Vector3f> scale;

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
            position = new Sync<Vector3f>(this, newRefIds);
            scale = new Sync<Vector3f>(this, newRefIds);
            scale.value = Vector3f.One;
            rotation = new Sync<Quaternionf>(this, newRefIds);
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

        public void addToRenderQueue(RenderQueue gu, Vector3f playpos)
        {
            if (!enabled.value)
            {
                return;
            }
            foreach (object comp in _components)
            {
                if ((Renderable)comp != null)
                {
                    gu.Add(((Renderable)comp), playpos.ToSystemNumrics());
                }
            }
            foreach(Entity child in _children)
            {
                child.addToRenderQueue(gu, playpos);
            }
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
