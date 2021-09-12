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
using g3;
namespace RhubarbEngine.Components.Physics.Colliders
{

	[Category(new string[] { "Physics/Colliders" })]
	public abstract class Collider : Component
	{
		public RigidBody collisionObject;

		public Sync<RCollisionFilterGroups> group;

		public Sync<RCollisionFilterGroups> mask;

		public Sync<float> mass;

		public Sync<bool> NoneStaticBody;

		public Driver<Vector3f> Scale;
		public Driver<Vector3f> Position;
		public Driver<Quaternionf> Rotation;

		public Sync<SyncLevel> SyncLevel;

		public override void inturnalSyncObjs(bool newRefIds)
		{
			base.inturnalSyncObjs(newRefIds);
			group = new Sync<RCollisionFilterGroups>(this, newRefIds, RCollisionFilterGroups.AllFilter);
			mask = new Sync<RCollisionFilterGroups>(this, newRefIds, RCollisionFilterGroups.AllFilter);
			mass = new Sync<float>(this, newRefIds);
			group.Changed += UpdateListner;
			mask.Changed += UpdateListner;
			mass.Changed += UpdateMassListner;
			entity.EnabledChanged += Enabled_Changed;
			NoneStaticBody = new Sync<bool>(this, newRefIds);
			Scale = new Driver<Vector3f>(this, newRefIds);
			Position = new Driver<Vector3f>(this, newRefIds);
			Rotation = new Driver<Quaternionf>(this, newRefIds);
			NoneStaticBody.Changed += NoneStaticBody_Changed;
			SyncLevel = new Sync<SyncLevel>(this, newRefIds, Physics.SyncLevel.Local);
			entity.OnPhysicsDisableder += Entity_onPhysicsDisableder;
		}

		private void Entity_onPhysicsDisableder(bool obj)
		{
			if (!_added)
            {
                return;
            }

            if (!NoneStaticBody.Value)
            {
                return;
            }

            if (obj)
			{
				collisionObject.ForceActivationState(ActivationState.DisableSimulation);
			}
			else
			{
				collisionObject.ForceActivationState(ActivationState.ActiveTag);
				collisionObject.Activate(true);
			}
		}

		private void NoneStaticBody_Changed(IChangeable obj)
		{
			if (!Scale.Linked)
			{
				Scale.SetDriveTarget(entity.scale);
			}
			if (!Position.Linked)
			{
				Position.SetDriveTarget(entity.position);
			}
			if (!Rotation.Linked)
			{
				Rotation.SetDriveTarget(entity.rotation);
			}
			BuildCollissionObject(collisionObject);
		}

		private bool _added = false;

		private void Enabled_Changed()
		{
			if (!entity.IsEnabled)
			{
				if (!_added)
                {
                    return;
                }

                _added = false;
				if (collisionObject == null)
                {
                    return;
                }

                world.PhysicsWorld.RemoveCollisionObject(collisionObject);
			}
			else
			{
				if (_added)
                {
                    return;
                }

                _added = true;
				if (collisionObject == null)
                {
                    return;
                }

                world.PhysicsWorld.AddCollisionObject(collisionObject, (int)group.Value, (int)mask.Value);
			}
		}

		public void StartShape(CollisionShape shape)
		{
			BuildCollissionObject(LocalCreateRigidBody(mass.Value, CastMet(entity.GlobalTrans()), shape));
		}
		public virtual void BuildShape()
		{

		}
		public override void onLoaded()
		{
			base.onLoaded();
			entity.GlobalTransformChangePhysics += UpdateTrans;
		}

		private void UpdateListner(IChangeable val)
		{
			BuildCollissionObject(collisionObject);
		}
		private void UpdateMassListner(IChangeable val)
		{
			var isDynamic = (mass.Value != 0.0f);
			var localInertia = isDynamic ? collisionObject.CollisionShape.CalculateLocalInertia(mass.Value) : BulletSharp.Math.Vector3.Zero;
			collisionObject.SetMassProps(mass.Value, localInertia);
			collisionObject.Activate(true);
		}
		public RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
		{
			//rigidbody is dynamic if and only if mass is non zero, otherwise static
			var isDynamic = (mass != 0.0f);
			var localInertia = isDynamic ? shape.CalculateLocalInertia(mass) : BulletSharp.Math.Vector3.Zero;

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

		public void BuildCollissionObject(RigidBody newCol)
		{
			if (collisionObject != null)
			{
				if (_added)
				{
					_added = false;
					world.PhysicsWorld.RemoveCollisionObject(collisionObject);
					world.PhysicsWorld.RemoveCollisionObject(collisionObject);
				}
				collisionObject = null;
			}
			if (newCol != null)
			{
				newCol.UserObject = this;
				if (entity.enabled.Value && entity.parentEnabled)
				{
					_added = true;
					if (NoneStaticBody.Value)
					{
						world.PhysicsWorld.AddRigidBody(newCol, (int)group.Value, (int)mask.Value);
					}
					else
					{
						world.PhysicsWorld.AddCollisionObject(newCol, (int)group.Value, (int)mask.Value);
					}
				}
			}
			else
			{
				_added = false;
			}
			collisionObject = newCol;
		}

		public void UpdateTrans(Matrix4x4 val)
		{
			if (collisionObject == null)
            {
                return;
            }

            collisionObject.Activate(true);
			collisionObject.WorldTransform = CastMet(val);
		}

		public static Matrix CastMet(Matrix4x4 matrix4X4)
		{
			var t = new Matrix(
				(double)matrix4X4.M11, (double)matrix4X4.M12, (double)matrix4X4.M13, (double)matrix4X4.M14,
				(double)matrix4X4.M21, (double)matrix4X4.M22, (double)matrix4X4.M23, (double)matrix4X4.M24,
				(double)matrix4X4.M31, (double)matrix4X4.M32, (double)matrix4X4.M33, (double)matrix4X4.M34,
				(double)matrix4X4.M41, (double)matrix4X4.M42, (double)matrix4X4.M43, (double)matrix4X4.M44);
			return t;
		}
		public static Matrix4x4 CastMet(Matrix matrix4X4)
		{
			var t = new Matrix4x4(
				(float)matrix4X4.M11, (float)matrix4X4.M12, (float)matrix4X4.M13, (float)matrix4X4.M14,
				(float)matrix4X4.M21, (float)matrix4X4.M22, (float)matrix4X4.M23, (float)matrix4X4.M24,
				(float)matrix4X4.M31, (float)matrix4X4.M32, (float)matrix4X4.M33, (float)matrix4X4.M34,
				(float)matrix4X4.M41, (float)matrix4X4.M42, (float)matrix4X4.M43, (float)matrix4X4.M44);
			return t;
		}
		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (!NoneStaticBody.Value)
            {
                return;
            }

            switch (SyncLevel.Value)
			{
				case Physics.SyncLevel.Local:
					LocalPosSync();
					break;
				case Physics.SyncLevel.SyncUpdate:
					SyncUpdate();
					break;
				case Physics.SyncLevel.ManagingUser:
					UserPosSync(entity.Manager);
					break;
				case Physics.SyncLevel.CreatingUser:
					UserPosSync(entity.CreatingUser);
					break;
				case Physics.SyncLevel.HostUser:
					UserPosSync(world.HostUser);
					break;
				default:
					Logger.Log("SyncLevel Does Not Exists");
					break;
			}
		}

		private void SyncUpdate()
		{
			Logger.Log("This Is not Done");
		}

		private void LocalPosSync()
		{
			if (entity.PhysicsDisabled)
            {
                return;
            }

            var newMat = CastMet(collisionObject.WorldTransform);
			entity.SetGlobalTrans(newMat, false);
		}
#pragma warning disable IDE0060 // Remove unused parameter
        private void UserPosSync(User user)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			Logger.Log("This Is not Done");
		}

		public Collider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public Collider()
		{
		}
	}
}
