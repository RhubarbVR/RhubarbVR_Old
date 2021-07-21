using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;

namespace RhubarbEngine.Components.Physics.Colliders
{
    [Flags]
    public enum RCollisionFilterGroups
    {
        AllFilter = -1,
        None = 0,
        DefaultFilter = 1,
        StaticFilter = 2,
        KinematicFilter = 4,
        DebrisFilter = 8,
        SensorTrigger = 16,
        CharacterFilter = 32,
        Custome1 = 64,
        Custome2 = 128,
        Custome3 = 256,
        Custome4 = 512,
        Custome5 = 1024,

    }

    [Category(new string[] { "Physics/Colliders" })]
    public abstract class Collider: Component
    {
        public RigidBody collisionObject;

        public Sync<RCollisionFilterGroups> group;

        public Sync<RCollisionFilterGroups> mask;

        public Sync<float> mass;

        public override void inturnalSyncObjs(bool newRefIds)
        {
            base.inturnalSyncObjs(newRefIds);
            group = new Sync<RCollisionFilterGroups>(this, newRefIds);
            mask = new Sync<RCollisionFilterGroups>(this, newRefIds);
            mask.value = RCollisionFilterGroups.StaticFilter;
            group.value = RCollisionFilterGroups.StaticFilter;
            mass = new Sync<float>(this, newRefIds);
            group.Changed += updateListner;
            mask.Changed += updateListner;
            mass.Changed += updateMassListner;
        }
        public void startShape(CollisionShape shape)
        {
            LocalCreateRigidBody(mass.value, CastMet(entity.globalTrans()), shape);
            
        }
        public virtual void BuildShape()
        {

        }
        public override void onLoaded()
        {
            base.onLoaded();
            entity.GlobalTransformChange += UpdateTrans;
        }

        private void updateListner(IChangeable val)
        {
            buildCollissionObject(collisionObject);
        }
        private void updateMassListner(IChangeable val)
        {
            bool isDynamic = (mass.value != 0.0f);
            BulletSharp.Math.Vector3 localInertia = isDynamic ? collisionObject.CollisionShape.CalculateLocalInertia(mass.value) : BulletSharp.Math.Vector3.Zero;
            collisionObject.SetMassProps(mass.value, localInertia);
        }
        public RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
        {
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            bool isDynamic = (mass != 0.0f);
            BulletSharp.Math.Vector3 localInertia = isDynamic ? shape.CalculateLocalInertia(mass) : BulletSharp.Math.Vector3.Zero;

            using (var rbInfo = new RigidBodyConstructionInfo(mass, null, shape, localInertia))
            {
                var body = new RigidBody(rbInfo)
                {
                    ContactProcessingThreshold = 0.0f,
                    WorldTransform = startTransform
                };
                return body;
            }
        }

        public void buildCollissionObject(RigidBody newCol)
        {
            if (collisionObject != null)
            {
                world.physicsWorld.RemoveCollisionObject(collisionObject);
            }
            world.physicsWorld.AddCollisionObject(newCol, (int)group.value, (int)mask.value);
            collisionObject = newCol;
        }

        public void UpdateTrans(Matrix4x4 val)
        {
            if (collisionObject == null) return;
            collisionObject.WorldTransform = CastMet(val);
        }

        public static Matrix CastMet(Matrix4x4 matrix4X4)
        {
            var t = new Matrix(
                (double)matrix4X4.M11, (double)matrix4X4.M12, (double)matrix4X4.M13, (double)matrix4X4.M14,
                (double)matrix4X4.M21, (double)matrix4X4.M22, (double)matrix4X4.M23, (double)matrix4X4.M24,
                (double)matrix4X4.M31, (double)matrix4X4.M32, (double)matrix4X4.M33, (double)matrix4X4.M34,
                (double)matrix4X4.M41, (double)matrix4X4.M42, (double)matrix4X4.M43, (double)matrix4X4.M44);
            t.Decompose(out BulletSharp.Math.Vector3 scale, out BulletSharp.Math.Quaternion rot, out BulletSharp.Math.Vector3 trans);
            Logger.Log(trans.ToString() +" T: "+ rot.ToString()+ "R:"+scale.ToString()+"S:");
            return t;
        }

        public Collider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Collider()
        {
        }
    }
}
