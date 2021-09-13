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
using RNumerics;
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

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			grabbableHolder = new SyncRef<GrabbableHolder>(this, newRefIds);
			grabbableHolder.Changed += GrabbableHolder_Changed;
			grabbingUser = new SyncRef<User>(this, newRefIds);
			lastParent = new SyncRef<Entity>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (grabbingUser.Target != World.LocalUser)
				return;
			if (LaserGrabbed && (grabbableHolder.Target != null))
			{
				Vector3d newpos = Vector3d.Zero;
				switch (grabbableHolder.Target.source.Value)
				{
					case InteractionSource.LeftLaser:
						newpos = (offset + Input.LeftLaser.pos);
						break;
					case InteractionSource.RightLaser:
						newpos = (offset + Input.RightLaser.pos);
						break;
					case InteractionSource.HeadLaser:
						newpos = (offset + Input.RightLaser.pos);
						break;
					default:
						break;
				}
				Entity.SetGlobalPos(new Vector3f(newpos.x, newpos.y, newpos.z));
			}
			volas = (((lastValue - Entity.GlobalPos()) * (1 / (float)Engine.platformInfo.deltaSeconds)) + volas) / 2;
			lastValue = Entity.GlobalPos();

		}

		private void GrabbableHolder_Changed(IChangeable obj)
		{
			if (grabbableHolder.Target == null)
			{
				Entity.RemovePhysicsDisableder(this);
			}
			else
			{
				Entity.AddPhysicsDisableder(this);

			}
		}

		public override void Dispose()
		{
			base.Dispose();
			Entity.RemovePhysicsDisableder(this);
		}

		public void Drop()
		{
			if (World.LocalUser != grabbingUser.Target)
				return;
			grabbingUser.Target = null;
			Entity.SetParent(lastParent.Target);
			Entity.SendDrop(false, grabbableHolder.Target, true);
			grabbableHolder.Target = null;
			foreach (var item in Entity.GetAllComponents<Collider>())
			{
				if (item.NoneStaticBody.Value && (item.collisionObject != null))
				{
					item.collisionObject.LinearVelocity = new BulletSharp.Math.Vector3(-volas.x, -volas.y, -volas.z);
					item.collisionObject.AngularVelocity = new BulletSharp.Math.Vector3(volas.x / 10, volas.y / 10, volas.z / 10);
				}
			}

		}


		public override void OnLoaded()
		{
			base.OnLoaded();
			Entity.OnGrip += Entity_onGrip;
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
				lastParent.Target = Entity.parent.Target;
			}
			LaserGrabbed = Laser;
			if (LaserGrabbed)
			{
				var laserpos = Vector3d.Zero;
				switch (obj.source.Value)
				{
					case InteractionSource.LeftLaser:
						laserpos = Input.LeftLaser.pos;
						Input.LeftLaser.Lock();
						break;
					case InteractionSource.RightLaser:
						laserpos = Input.RightLaser.pos;
						Input.RightLaser.Lock();
						break;
					case InteractionSource.HeadLaser:
						laserpos = Input.RightLaser.pos;
						Input.RightLaser.Lock();
						break;
					default:
						break;
				}
				offset = (laserpos - Entity.GlobalPos());
			}
			Entity.Manager = World.LocalUser;
			grabbableHolder.Target = obj;
			grabbingUser.Target = World.LocalUser;
			Entity.SetParent(obj.holder.Target);
		}

		public void RemoteGrab()
		{
			if (World.lastHolder == null)
				return;
			Entity_onGrip(World.lastHolder, true);
		}

		public Grabbable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public Grabbable()
		{
		}
	}
}
