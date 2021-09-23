using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using System.Numerics;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using Veldrid;
using Veldrid.Sdl2;
using System.Runtime.InteropServices;
using RNumerics;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.Users
{
	[Category(new string[] { "Users" })]
	public class UserRoot : Component
	{

		private readonly float _moveSpeed = 10.0f;

		public SyncRef<Entity> Head;

		public SyncRef<Entity> LeftHand;
		public SyncRef<Entity> RightHand;


		public SyncRef<User> user;

		public Driver<Vector3f> posDriver;
		public Driver<Quaternionf> rotDriver;
		public Driver<Vector3f> scaleDriver;

        public Matrix4x4 Viewpos
        {
            get
            {
                return Entity.GlobalTrans();
            }
        }

        public Matrix4x4 Headpos
        {
            get
            {
                return Head.Target?.GlobalTrans() ?? Entity.GlobalTrans();
            }
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
			Head = new SyncRef<Entity>(this, newRefIds);
			LeftHand = new SyncRef<Entity>(this, newRefIds);
			RightHand = new SyncRef<Entity>(this, newRefIds);
			user = new SyncRef<User>(this, newRefIds);
			posDriver = new Driver<Vector3f>(this, newRefIds);
			rotDriver = new Driver<Quaternionf>(this, newRefIds);
			scaleDriver = new Driver<Vector3f>(this, newRefIds);
		}

		public override void OnLoaded()
		{
			Entity.persistence.Value = false;
			Logger.Log("Loaded Char");
		}

		public override void OnAttach()
		{
			base.OnAttach();
			posDriver.SetDriveTarget(Entity.position);
			rotDriver.SetDriveTarget(Entity.rotation);
			scaleDriver.SetDriveTarget(Entity.scale);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (World.Userspace)
			{
				return;
			};
			if (World.LocalUser == user.Target)
			{
				if (Input.MainWindows.GetKeyDown(Key.F7))
				{
					Console.WriteLine(Entity.GlobalPos());
				}
				if (Input.IsKeyboardinuse)
                {
                    return;
                }

                var deltaSeconds = (float)World.worldManager.engine.PlatformInfo.deltaSeconds;
				var sprintFactor = World.worldManager.engine.InputManager.MainWindows.GetKey(Key.ControlLeft) || Engine.InputManager.PrimaryPress()
				   ? 0.1f
				   : World.worldManager.engine.InputManager.MainWindows.GetKey(Key.ShiftLeft)
				   ? 2.5f
					 : 1f;
				var motionDir = Vector3.Zero;
				var leftvraix = Engine.InputManager.Axis(RhubarbEngine.Input.Creality.Left);
				var Rightvraix = Engine.InputManager.Axis(RhubarbEngine.Input.Creality.Right);
				motionDir -= (LeftHand.Target.rotation.Value.AxisZ * leftvraix.y).ToSystemNumrics();
				motionDir -= (RightHand.Target.rotation.Value.AxisZ * Rightvraix.y).ToSystemNumrics();

				var lookRotation = Quaternion.CreateFromYawPitchRoll(leftvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
				lookRotation *= Quaternion.CreateFromYawPitchRoll(Rightvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
				float e = World.worldManager.engine.InputManager.MainWindows.GetKey(Key.X) ? 0 : 1;
				e += World.worldManager.engine.InputManager.MainWindows.GetKey(Key.Z) ? 0 : -1;
				lookRotation *= Quaternion.CreateFromYawPitchRoll(e * -5f * deltaSeconds, 0.0f, 0.0f);

				var temp = World.UserRoot.Head.Target.rotation.Value * Vector3f.AxisZ;
				var HeadFacingDirection = new Vector3f(temp.x, 0, temp.z).Normalized;
				var looke = Quaternionf.FromTo(Vector3f.AxisZ, HeadFacingDirection);
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.A))
				{
					motionDir += -looke.AxisX.ToSystemNumrics();
				}
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.D))
				{
					motionDir += looke.AxisX.ToSystemNumrics();
				}
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.W))
				{
					motionDir += -looke.AxisZ.ToSystemNumrics();
				}
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.S))
				{
					motionDir += looke.AxisZ.ToSystemNumrics();
				}
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.Q))
				{
					motionDir += -Vector3.UnitY;
				}
				if (World.worldManager.engine.InputManager.MainWindows.GetKey(Key.E))
				{
					motionDir += Vector3.UnitY;
				}
				if (motionDir != Vector3.Zero || lookRotation != default)
				{

					motionDir = Vector3.Transform(motionDir, lookRotation);
					if (float.IsNaN(motionDir.X) || float.IsNaN(motionDir.Y) || float.IsNaN(motionDir.Z))
					{
						return;
					}

					var addTo = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(lookRotation) * Matrix4x4.CreateTranslation(motionDir * _moveSpeed * sprintFactor * deltaSeconds);

					var newtraans = addTo * Entity.GlobalTrans();
					Entity.SetGlobalTrans(newtraans);


					var userpos = World.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserPos");
					var userrot = World.LocalUser.FindOrCreateUserStream<SyncStream<Quaternionf>>("UserRot");
					var userscale = World.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserScale");
					Matrix4x4.Decompose(Entity.GlobalTrans(), out var scale, out var rotation, out var translation);

					userpos.Value = new Vector3f(translation.X, translation.Y, translation.Z);
					userrot.Value = new Quaternionf(rotation.X, rotation.Y, rotation.Z, rotation.W);
					userscale.Value = new Vector3f(scale.X, scale.Y, scale.Z);
					Engine.WorldManager.privateOverlay.UserRoot.Entity.SetGlobalTrans(Entity.GlobalTrans());
				}
			}
			else
			{
				if (user.Target != null)
				{
					var temp = user.Target;
					var userpos = temp.FindUserStream<SyncStream<Vector3f>>("UserPos");
					var userrot = temp.FindUserStream<SyncStream<Quaternionf>>("UserRot");
					var userscale = temp.FindUserStream<SyncStream<Vector3f>>("UserScale");
					try
					{
						var value = Matrix4x4.CreateScale(userscale.Value.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(userrot.Value.ToSystemNumric()) * Matrix4x4.CreateTranslation(userpos.Value.ToSystemNumrics());
						Entity.SetGlobalTrans(value);
					}
					catch
					{

					}

				}
			}
		}
		public UserRoot(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public UserRoot()
		{
		}
	}
}
