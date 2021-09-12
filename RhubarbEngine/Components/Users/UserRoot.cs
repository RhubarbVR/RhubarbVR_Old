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
                return entity.GlobalTrans();
            }
        }

        public Matrix4x4 Headpos
        {
            get
            {
                return Head.Target?.GlobalTrans() ?? entity.GlobalTrans();
            }
        }

        public override void buildSyncObjs(bool newRefIds)
		{
			Head = new SyncRef<Entity>(this, newRefIds);
			LeftHand = new SyncRef<Entity>(this, newRefIds);
			RightHand = new SyncRef<Entity>(this, newRefIds);
			user = new SyncRef<User>(this, newRefIds);
			posDriver = new Driver<Vector3f>(this, newRefIds);
			rotDriver = new Driver<Quaternionf>(this, newRefIds);
			scaleDriver = new Driver<Vector3f>(this, newRefIds);
		}

		public override void onLoaded()
		{
			entity.persistence.Value = false;
			logger.Log("Loaded Char");
		}

		public override void OnAttach()
		{
			base.OnAttach();
			posDriver.SetDriveTarget(entity.position);
			rotDriver.SetDriveTarget(entity.rotation);
			scaleDriver.SetDriveTarget(entity.scale);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (world.Userspace)
			{
				return;
			};
			if (world.LocalUser == user.Target)
			{
				if (input.mainWindows.GetKeyDown(Key.F7))
				{
					Console.WriteLine(entity.GlobalPos());
				}
				if (input.IsKeyboardinuse)
                {
                    return;
                }

                var deltaSeconds = (float)world.worldManager.engine.platformInfo.deltaSeconds;
				var sprintFactor = world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ControlLeft) || engine.inputManager.PrimaryPress()
				   ? 0.1f
				   : world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ShiftLeft)
				   ? 2.5f
					 : 1f;
				var motionDir = Vector3.Zero;
				var leftvraix = engine.inputManager.Axis(Input.Creality.Left);
				var Rightvraix = engine.inputManager.Axis(Input.Creality.Right);
				motionDir -= (LeftHand.Target.rotation.Value.AxisZ * leftvraix.y).ToSystemNumrics();
				motionDir -= (RightHand.Target.rotation.Value.AxisZ * Rightvraix.y).ToSystemNumrics();

				var lookRotation = Quaternion.CreateFromYawPitchRoll(leftvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
				lookRotation *= Quaternion.CreateFromYawPitchRoll(Rightvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
				float e = (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.X)) ? 0 : 1;
				e += (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.Z)) ? 0 : -1;
				lookRotation *= Quaternion.CreateFromYawPitchRoll(e * -5f * deltaSeconds, 0.0f, 0.0f);

				var temp = world.UserRoot.Head.Target.rotation.Value * Vector3f.AxisZ;
				var HeadFacingDirection = new Vector3f(temp.x, 0, temp.z).Normalized;
				var looke = Quaternionf.FromTo(Vector3f.AxisZ, HeadFacingDirection);
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.A))
				{
					motionDir += -looke.AxisX.ToSystemNumrics();
				}
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.D))
				{
					motionDir += looke.AxisX.ToSystemNumrics();
				}
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.W))
				{
					motionDir += -looke.AxisZ.ToSystemNumrics();
				}
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.S))
				{
					motionDir += looke.AxisZ.ToSystemNumrics();
				}
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.Q))
				{
					motionDir += -Vector3.UnitY;
				}
				if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.E))
				{
					motionDir += Vector3.UnitY;
				}
				if (motionDir != Vector3.Zero || lookRotation != default)
				{

					motionDir = Vector3.Transform(motionDir, lookRotation);
					if ((motionDir.X == float.NaN) || (motionDir.Y == float.NaN) || (motionDir.Z == float.NaN))
					{
						return;
					}

					var addTo = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(lookRotation) * Matrix4x4.CreateTranslation(motionDir * _moveSpeed * sprintFactor * deltaSeconds);

					var newtraans = addTo * entity.GlobalTrans();
					entity.SetGlobalTrans(newtraans);


					var userpos = world.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserPos");
					var userrot = world.LocalUser.FindOrCreateUserStream<SyncStream<Quaternionf>>("UserRot");
					var userscale = world.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserScale");
					Matrix4x4.Decompose(entity.GlobalTrans(), out var scale, out var rotation, out var translation);

					userpos.Value = new Vector3f(translation.X, translation.Y, translation.Z);
					userrot.Value = new Quaternionf(rotation.X, rotation.Y, rotation.Z, rotation.W);
					userscale.Value = new Vector3f(scale.X, scale.Y, scale.Z);
					engine.worldManager.privateOverlay.UserRoot.entity.SetGlobalTrans(entity.GlobalTrans());
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
						entity.SetGlobalTrans(value);
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
