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
    [Category(new string[] { "Interaction" })]
    public class Grabbable : Component, IPhysicsDisabler, IVelocityRequest
	{
		public SyncRef<Entity> lastParent;

        public Sync<bool> CanNotDestroy;

        public SyncRef<User> grabbingUser;

		public SyncRef<GrabbableHolder> grabbableHolder;

		Vector3 _offset;

		public bool LaserGrabbed;

        public bool Grabbed
        {
            get
            {
                return (grabbableHolder.Target is not null) && (grabbingUser.Target is not null);
            }
        }


        public void DestroyGrabbedObject()
        {
            if (!CanNotDestroy.Value)
            {
                Entity.Destroy();
            }
        }

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			grabbableHolder = new SyncRef<GrabbableHolder>(this, newRefIds);
			grabbableHolder.Changed += GrabbableHolder_Changed;
			grabbingUser = new SyncRef<User>(this, newRefIds);
			lastParent = new SyncRef<Entity>(this, newRefIds);
            CanNotDestroy = new Sync<bool>(this, newRefIds);
        }


		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (grabbingUser.Target != World.LocalUser)
            {
                return;
            }

            if (Grabbed)
            {
                var scaler = 0f;
                if (Input.MainWindows.GetKey(Veldrid.Key.ShiftLeft))
                {
                    scaler += Input.MainWindows.FrameSnapshot.WheelDelta / 3;
                }
                if (scaler != 0)
                {
                    var newscale = (Entity.scale.Value + scaler);
                    if(!(newscale < Vector3f.Zero))
                    {
                        Entity.scale.Value = newscale;
                    }
                }
                var isClickingPrime = false;
                switch (grabbableHolder.Target.source.Value)
                {
                    case InteractionSource.LeftLaser:
                        isClickingPrime = Input.PrimaryPress(RhubarbEngine.Input.Creality.Left);
                        break;
                    case InteractionSource.RightLaser:
                        isClickingPrime = Input.PrimaryPress(RhubarbEngine.Input.Creality.Right);
                        break;
                    case InteractionSource.HeadLaser:
                        isClickingPrime = Input.MainWindows.GetMouseButton(Veldrid.MouseButton.Left);
                       break;
                    default:
                        break;
                }
                if (isClickingPrime)
                {
                    Entity.RotateToUpVector(World.UserRoot?.Entity.parent.Target??World.RootEntity);
                }
            }

            if (LaserGrabbed && (grabbableHolder.Target != null))
			{
				var newpos = Vector3.Zero;
				switch (grabbableHolder.Target.source.Value)
				{
					case InteractionSource.LeftLaser:
						newpos = new Vector3(Input.LeftLaser.Pos.X, Input.LeftLaser.Pos.Y, Input.LeftLaser.Pos.Z) - _offset;
						break;
					case InteractionSource.RightLaser:
						newpos = new Vector3(Input.RightLaser.Pos.X, Input.RightLaser.Pos.Y, Input.RightLaser.Pos.Z) - _offset;
						break;
					case InteractionSource.HeadLaser:
						newpos = new Vector3(Input.RightLaser.Pos.X, Input.RightLaser.Pos.Y, Input.RightLaser.Pos.Z) - _offset;
						break;
					default:
						break;
				}
				Entity.SetGlobalPos(new Vector3f(newpos.X, newpos.Y, newpos.Z));
			}

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
            try
            {
                grabbableHolder.Target.GrabbedObjects.Remove(this);
            }
            catch { }
            Entity.RemovePhysicsDisableder(this);
            base.Dispose();
        }

        public void Drop()
		{
			if (World.LocalUser != grabbingUser.Target)
            {
                return;
            }

            grabbingUser.Target = null;
			Entity.SetParent(lastParent.Target);
			Entity.SendDrop(false, grabbableHolder.Target, true);
            try
            {
                grabbableHolder.Target.GrabbedObjects.Remove(this);
            }
            catch { }
            grabbableHolder.Target = null;
			foreach (var item in Entity.GetAllComponents<Collider>())
			{
				if (item.NoneStaticBody.Value && (item.collisionObject != null))
				{
					item.collisionObject.LinearVelocity = new BulletSharp.Math.Vector3(-Entity.Velocity.x * 100, -Entity.Velocity.y * 100, -Entity.Velocity.z * 100);
					item.collisionObject.AngularVelocity = new BulletSharp.Math.Vector3(Entity.Velocity.x * 100, Entity.Velocity.y * 100, Entity.Velocity.z * 100);
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
            {
                return;
            }

            if (obj == null)
            {
                return;
            }

            if (obj.holder.Target == null)
            {
                return;
            }

            if (!Grabbed)
			{
				lastParent.Target = Entity.parent.Target;
			}
            if (Laser)
            {
                RhubarbEngine.Input.InteractionLaserSource lasere = null;
                switch (obj.source.Value)
                {
                    case InteractionSource.LeftLaser:
                        lasere = Input.LeftLaser;
                        break;
                    case InteractionSource.RightLaser:
                        lasere = Input.RightLaser;
                        break;
                    case InteractionSource.HeadLaser:
                        lasere = Input.RightLaser;
                        break;
                    default:
                        break;
                }
                if (lasere?.IsLocked ?? false)
                {
                    return;
                }
            }
			LaserGrabbed = Laser;
			if (LaserGrabbed)
			{
				var laserpos = Vector3.Zero;
				switch (obj.source.Value)
				{
					case InteractionSource.LeftLaser:
						laserpos = new Vector3(Input.LeftLaser.Pos.X, Input.LeftLaser.Pos.Y, Input.LeftLaser.Pos.Z);
						Input.LeftLaser.Lock();
						break;
					case InteractionSource.RightLaser:
						laserpos = new Vector3(Input.RightLaser.Pos.X, Input.RightLaser.Pos.Y, Input.RightLaser.Pos.Z);
						Input.RightLaser.Lock();
						break;
					case InteractionSource.HeadLaser:
						laserpos = new Vector3(Input.RightLaser.Pos.X, Input.RightLaser.Pos.Y, Input.RightLaser.Pos.Z);
						Input.RightLaser.Lock();
						break;
					default:
						break;
				}
				_offset = laserpos - Entity.GlobalPos().ToSystemNumrics();
			}
			Entity.Manager = World.LocalUser;
			grabbableHolder.Target = obj;
			grabbingUser.Target = World.LocalUser;
			Entity.SetParent(obj.holder.Target);
            try
            {
                grabbableHolder.Target.GrabbedObjects.Add(this);
            }
            catch { }
        }

		public void RemoteGrab(GrabbableHolder grabbableHolder)
		{
            if (grabbableHolder is not null)
            {
                Entity_onGrip(grabbableHolder, true);
            }
		}

		public Grabbable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public Grabbable()
		{
		}
	}
}
