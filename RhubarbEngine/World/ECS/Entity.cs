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
    public class Entity : Worker
    {
        public Sync<Vector3f> position;

        public Sync<Quaternionf> rotation;

        public Sync<Vector3f> scale;

        private Matrix4x4 cashedGlobalTrans = Matrix4x4.CreateScale(Vector3.One);

        private Matrix4x4 cashedLocalMatrix = Matrix4x4.CreateScale(Vector3.One);

        public SyncRef<Entity> parent;

        public Sync<string> name;

        public Sync<bool> enabled;

        public Sync<bool> persistence;

        public SyncObjList<Entity> _children;

        public SyncAbstractObjList<Component> _components;

        public Sync<int> remderlayer;

        public bool parentEnabled = true;
        public void parentEnabledChange(bool _parentEnabled)
        {
            if (_parentEnabled != enabled.value)
            {
                parentEnabled = _parentEnabled;
                foreach (Entity item in _children)
                {
                    item.parentEnabledChange(_parentEnabled);
                }
            }
        }

        public void onPersistenceChange(IChangeable newValue)
        {
            base.Persistent = persistence.value;
        }
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

        public Vector3f up { get
            {
                var mat = globalTrans();
                var e = new Vector3f(mat.M11 * Vector3f.AxisY.x + mat.M12 * Vector3f.AxisY.y + mat.M13 * Vector3f.AxisY.z, mat.M21 * Vector3f.AxisY.x + mat.M22 * Vector3f.AxisY.y + mat.M23 * Vector3f.AxisY.z, mat.M31 * Vector3f.AxisY.x + mat.M32 * Vector3f.AxisY.y + mat.M33 * Vector3f.AxisY.z);
                return e;

            }
        }

        public Matrix4x4 localTrans()
        {
            return cashedLocalMatrix;
        }

        public void setGlobalTrans(Matrix4x4 newtrans)
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (parent.target != null)
            {
                parentMatrix = parent.target.globalTrans();
            }
            Matrix4x4 newlocal = parentMatrix * newtrans;
            Matrix4x4.Decompose(newlocal, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            position.value = new Vector3f(newtranslation.X, newtranslation.Y, newtranslation.Z);
            rotation.value = new Quaternionf(newrotation.X, newrotation.Y, newrotation.Z, newrotation.W);
            scale.value = new Vector3f(newscale.X, newscale.Y, newscale.Z);
            cashedGlobalTrans = newtrans;
        }

        public Action<Matrix4x4> GlobalTransformChange;

        public void setLocalTrans(Matrix4x4 newtrans)
        {
            Matrix4x4.Decompose(newtrans, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
            position.value = new Vector3f(newtranslation.X, newtranslation.Y, newtranslation.Z);
            rotation.value = new Quaternionf(newrotation.X, newrotation.Y, newrotation.Z, newrotation.W);
            scale.value = new Vector3f(newscale.X, newscale.Y, newscale.Z);
        }



        public override void buildSyncObjs(bool newRefIds)
        {
            position = new Sync<Vector3f>(this, newRefIds);
            scale = new Sync<Vector3f>(this, newRefIds);
            scale.value = Vector3f.One;
            rotation = new Sync<Quaternionf>(this, newRefIds);
            name = new Sync<string>(this, newRefIds);
            enabled = new Sync<bool>(this, newRefIds);
            persistence = new Sync<bool>(this, newRefIds);
            persistence.value = true;
            _children = new SyncObjList<Entity>(this, newRefIds);
            _components = new SyncAbstractObjList<Component>(this, newRefIds);
            enabled.value = true;
            parent = new SyncRef<Entity>(this, newRefIds);
            remderlayer = new Sync<int>(this, newRefIds);
            remderlayer.value = (int)RemderLayers.normal;
            position.Changed += onTransChange;
            rotation.Changed += onTransChange;
            scale.Changed += onTransChange;
            enabled.Changed += onEnableChange;
            persistence.Changed += onPersistenceChange;
        }
        public void onTransChange(IChangeable newValue)
        {
            updateGlobalTrans();
        }

        public void onEnableChange(IChangeable newValue)
        {
            foreach (Entity item in _children)
            {
                item.parentEnabledChange(enabled.value);
            }
        }
        public void updateGlobalTrans()
        {
            Matrix4x4 parentMatrix = Matrix4x4.CreateScale(Vector3.One);
            if (parent.target != null)
            {
                parentMatrix = parent.target.globalTrans();
            }
            Matrix4x4 localMatrix = Matrix4x4.CreateScale(scale.value.x, scale.value.y, scale.value.z) * Matrix4x4.CreateFromQuaternion(rotation.value.ToSystemNumric()) * Matrix4x4.CreateTranslation(position.value.x, position.value.y, position.value.z);
            cashedGlobalTrans = localMatrix * parentMatrix;
            cashedLocalMatrix = localMatrix;
            GlobalTransformChange?.Invoke(cashedGlobalTrans);
            foreach (Entity entity in _children)
            {
                entity.updateGlobalTrans();
            }
        }
        [NoSync]
        [NoSave]
        public Entity addChild(string name = "Entity")
        {
            Entity val = _children.Add(true);
            val.parent.target = this;
            val.name.value = name;
            return val;
        }
        [NoSync]
        [NoSave]
        public T attachComponent<T>() where T: Component
        {
            T newcomp = (T)Activator.CreateInstance(typeof(T));
            _components.Add(newcomp);
            newcomp.OnAttach();
            return newcomp;
        }
        public override void onLoaded()
        {
            updateGlobalTrans();
        }

        public void addToRenderQueue(RenderQueue gu, Vector3 playpos, RemderLayers layer)
        {
            if (((int)layer & remderlayer.value) <= 0)
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
                {}
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
                if (comp.enabled.value)
                {
                    comp.CommonUpdate(startTime, Frame);
                }
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
