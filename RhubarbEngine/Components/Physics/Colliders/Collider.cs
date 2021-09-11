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
			group = new Sync<RCollisionFilterGroups>(this, newRefIds);
			mask = new Sync<RCollisionFilterGroups>(this, newRefIds);
			mask.value = RCollisionFilterGroups.AllFilter;
			group.value = RCollisionFilterGroups.AllFilter;
			mass = new Sync<float>(this, newRefIds);
			group.Changed += updateListner;
			mask.Changed += updateListner;
			mass.Changed += updateMassListner;
			entity.enabledChanged += Enabled_Changed;
			NoneStaticBody = new Sync<bool>(this, newRefIds);
			Scale = new Driver<Vector3f>(this, newRefIds);
			Position = new Driver<Vector3f>(this, newRefIds);
			Rotation = new Driver<Quaternionf>(this, newRefIds);
			NoneStaticBody.Changed += NoneStaticBody_Changed;
			SyncLevel = new Sync<SyncLevel>(this, newRefIds);
			SyncLevel.value = Physics.SyncLevel.Local;
			entity.onPhysicsDisableder += Entity_onPhysicsDisableder;
		}

		private void Entity_onPhysicsDisableder(bool obj)
		{
			if (!Added)
				return;
			if (!NoneStaticBody.value)
				return;
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
				Scale.setDriveTarget(entity.scale);
			}
			if (!Position.Linked)
			{
				Position.setDriveTarget(entity.position);
			}
			if (!Rotation.Linked)
			{
				Rotation.setDriveTarget(entity.rotation);
			}
			buildCollissionObject(collisionObject);
		}

		private bool Added = false;

		private void Enabled_Changed()
		{
			if (!entity.isEnabled)
			{
				if (!Added)
					return;
				Added = false;
				if (collisionObject == null)
					return;
				world.physicsWorld.RemoveCollisionObject(collisionObject);
			}
			else
			{
				if (Added)
					return;
				Added = true;
				if (collisionObject == null)
					return;
				world.physicsWorld.AddCollisionObject(collisionObject, (int)group.value, (int)mask.value);
			}
		}

		public void startShape(CollisionShape shape)
		{
			buildCollissionObject(LocalCreateRigidBody(mass.value, CastMet(entity.globalTrans()), shape));
		}
		public virtual void BuildShape()
		{

		}
		public override void onLoaded()
		{
			base.onLoaded();
			entity.GlobalTransformChangePhysics += UpdateTrans;
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
			collisionObject.Activate(true);
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
				if (Added)
				{
					Added = false;
					world.physicsWorld.RemoveCollisionObject(collisionObject);
					world.physicsWorld.RemoveCollisionObject(collisionObject);
				}
				collisionObject = null;
			}
			if (newCol != null)
			{
				newCol.UserObject = this;
				if (entity.enabled.value && entity.parentEnabled)
				{
					Added = true;
					if (NoneStaticBody.value)
					{
						world.physicsWorld.AddRigidBody(newCol, (int)group.value, (int)mask.value);
					}
					else
					{
						world.physicsWorld.AddCollisionObject(newCol, (int)group.value, (int)mask.value);
					}
				}
			}
			else
			{
				Added = false;
			}
			collisionObject = newCol;
		}

		public void UpdateTrans(Matrix4x4 val)
		{
			if (collisionObject == null)
				return;
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
			if (!NoneStaticBody.value)
				return;
			switch (SyncLevel.value)
			{
				case Physics.SyncLevel.Local:
					LocalPosSync();
					break;
				case Physics.SyncLevel.SyncUpdate:
					SyncUpdate();
					break;
				case Physics.SyncLevel.ManagingUser:
					UserPosSync(entity.manager);
					break;
				case Physics.SyncLevel.CreatingUser:
					UserPosSync(entity.CreatingUser);
					break;
				case Physics.SyncLevel.HostUser:
					UserPosSync(world.hostUser);
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
				return;
			var newMat = CastMet(collisionObject.WorldTransform);
			entity.setGlobalTrans(newMat, false);
		}
		private void UserPosSync(User user)
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
