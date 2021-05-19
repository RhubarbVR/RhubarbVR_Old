using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.Render;
using RhubarbEngine.Components;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using System.Numerics;

namespace RhubarbEngine.World.ECS
{
    public class Entity: Worker
    {
        public Sync<Vector3f> position;

        public Sync<Quaternionf> rotation;

        public Sync<Vector3f> scale;

        private Matrix4x4 cashedGlobalTrans = Matrix4x4.CreateScale(Vector3.One);

        public SyncRef<Entity> parent;

        public Sync<string> name;

        public Sync<bool> enabled;

        private SyncObjList<Entity> _children;

        private SyncAbstractObjList<Component> _components;

        public override void inturnalSyncObjs(bool newRefIds)
        {
            world.addWorldEntity(this);
        }

        public Vector3f globalPos()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return new Vector3f(translation.X, translation.Y, translation.Z);
        }
        public Quaternionf globalRot()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return new Quaternionf(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }
        public Vector3f globalScale()
        {
            Matrix4x4.Decompose(cashedGlobalTrans, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return new Vector3f(scale.X, scale.Y, scale.Z);
        }

        public Matrix4x4 globalTrans()
        {
            return cashedGlobalTrans;
        }
        public void setGlobalTrans(Matrix4x4 newtrans)
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (parent.target != null)
            {
                parentMatrix = parent.target.globalTrans();
            }
            Matrix4x4 newlocal = Matrix4x4.Subtract(newtrans, parentMatrix);
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            position.value = new Vector3f(newtranslation.X, newtranslation.Y, newtranslation.Z);
            rotation.value = new Quaternionf(newrotation.X, newrotation.Y, newrotation.Z, newrotation.W);
            scale.value = new Vector3f(newscale.X, newscale.Y, newscale.Z);
            cashedGlobalTrans = newtrans;
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

            position.Changed += onTransChange;
            rotation.Changed += onTransChange;
            scale.Changed += onTransChange;
        }
        public void onTransChange(IChangeable newValue)
        {
            updateGlobalTrans();
        }

        public void updateGlobalTrans()
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (parent.target != null)
            {
                parentMatrix = parent.target.globalTrans();
            }
            Matrix4x4 localMatrix = (new Matrix4x4())+ Matrix4x4.CreateTranslation(position.value.x, position.value.y, position.value.z) + Matrix4x4.CreateScale(scale.value.x, scale.value.y, scale.value.z) + Matrix4x4.CreateFromQuaternion(new Quaternion(rotation.value.x, rotation.value.y, rotation.value.z, rotation.value.w));
            cashedGlobalTrans = Matrix4x4.Add(localMatrix, parentMatrix);
            foreach (Entity entity in _children)
            {
                entity.updateGlobalTrans();
            }
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
            if(_components.Count() <= 0)
            {
                MeshRender val = attachComponent<MeshRender>();
                val.source.target = attachComponent<BoxMesh>();
                val.onLoaded();
            }
        }

        public void addToRenderQueue(RenderQueue gu, Vector3 playpos)
        {
            if (!enabled.value)
            {
                return;
            }
            foreach (object comp in _components)
            {
                try
                {
                    gu.Add(((Renderable)comp), playpos);
                }
                catch
                {

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
