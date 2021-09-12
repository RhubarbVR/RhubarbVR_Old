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
using g3;
using System.Numerics;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.Interaction
{
	public class Grabbable : Component, IPhysicsDisableder
	{
		public SyncRef<Entity> lastParent;

		public SyncRef<User> grabbingUser;

		public SyncRef<GrabbableHolder> grabbableHolder;

		Vector3d offset;

		public bool LaserGrabbed;

		public bool Grabbed => (grabbableHolder.Target != null) && (grabbingUser.Target != null);

		Vector3f lastValue;
		Vector3f volas;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			grabbableHolder = new SyncRef<GrabbableHolder>(this, newRefIds);
			grabbableHolder.Changed += GrabbableHolder_Changed;
			grabbingUser = new SyncRef<User>(this, newRefIds);
			lastParent = new SyncRef<Entity>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (grabbingUser.Target != world.LocalUser)
				return;
			if (LaserGrabbed && (grabbableHolder.Target != null))
			{
				Vector3d newpos = Vector3d.Zero;
				switch (grabbableHolder.Target.source.Value)
				{
					case InteractionSource.LeftLaser:
						newpos = (offset + input.LeftLaser.pos);
						break;
					case InteractionSource.RightLaser:
						newpos = (offset + input.RightLaser.pos);
						break;
					case InteractionSource.HeadLaser:
						newpos = (offset + input.RightLaser.pos);
						break;
					default:
						break;
				}
				entity.SetGlobalPos(new Vector3f(newpos.x, newpos.y, newpos.z));
			}
			volas = (((lastValue - entity.GlobalPos()) * (1 / (float)engine.platformInfo.deltaSeconds)) + volas) / 2;
			lastValue = entity.GlobalPos();

		}

		private void GrabbableHolder_Changed(IChangeable obj)
		{
			if (grabbableHolder.Target == null)
			{
				entity.RemovePhysicsDisableder(this);
			}
			else
			{
				entity.AddPhysicsDisableder(this);

			}
		}

		public override void Dispose()
		{
			base.Dispose();
			entity.RemovePhysicsDisableder(this);
		}

		public void Drop()
		{
			if (world.LocalUser != grabbingUser.Target)
				return;
			grabbingUser.Target = null;
			entity.SetParent(lastParent.Target);
			entity.SendDrop(false, grabbableHolder.Target, true);
			grabbableHolder.Target = null;
			foreach (var item in entity.GetAllComponents<Collider>())
			{
				if (item.NoneStaticBody.Value && (item.collisionObject != null))
				{
					item.collisionObject.LinearVelocity = new BulletSharp.Math.Vector3(-volas.x, -volas.y, -volas.z);
					item.collisionObject.AngularVelocity = new BulletSharp.Math.Vector3(volas.x / 10, volas.y / 10, volas.z / 10);
				}
			}

		}


		public override void onLoaded()
		{
			base.onLoaded();
			entity.OnGrip += Entity_onGrip;
		}

		private void Entity_onGrip(GrabbableHolder obj, bool Laser)
		{
			if (grabbableHolder.Target == obj)
				return;
			if (obj == null)
				return;
			if (obj.holder.Target == null)
				return;
			if (!Grabbed)
			{
				lastParent.Target = entity.parent.Target;
			}
			LaserGrabbed = Laser;
			if (LaserGrabbed)
			{
				var laserpos = Vector3d.Zero;
				switch (obj.source.Value)
				{
					case InteractionSource.LeftLaser:
						laserpos = input.LeftLaser.pos;
						input.LeftLaser.Lock();
						break;
					case InteractionSource.RightLaser:
						laserpos = input.RightLaser.pos;
						input.RightLaser.Lock();
						break;
					case InteractionSource.HeadLaser:
						laserpos = input.RightLaser.pos;
						input.RightLaser.Lock();
						break;
					default:
						break;
				}
				offset = (laserpos - entity.GlobalPos());
			}
			entity.Manager = world.LocalUser;
			grabbableHolder.Target = obj;
			grabbingUser.Target = world.LocalUser;
			entity.SetParent(obj.holder.Target);
		}

		public void RemoteGrab()
		{
			if (world.lastHolder == null)
				return;
			Entity_onGrip(world.lastHolder, true);
		}

		public Grabbable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public Grabbable()
		{
		}
	}
}
