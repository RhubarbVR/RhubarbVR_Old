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
using RhubarbEngine.Input;

namespace RhubarbEngine.Components.Users
{
	[Category(new string[] { "Users" })]
	public class Hand : Component
	{
		public Sync<Creality> creality;
		public SyncRef<UserRoot> userroot;

		public Driver<Vector3f> posDriver;
		public Driver<Quaternionf> rotDriver;
		public Driver<Vector3f> scaleDriver;

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
				Matrix4x4 val = input.GetPos(creality.Value);
				entity.SetLocalTrans(val);
				return;
			}
			if (userroot.Target == null)
			{
				return;
			}
			if (userroot.Target.user.Target == world.LocalUser)
			{
				Matrix4x4 val = input.GetPos(creality.Value);
				entity.SetLocalTrans(val);
				var userpos = world.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>($"Hand{creality.Value}Pos");
				var userrot = world.LocalUser.FindOrCreateUserStream<SyncStream<Quaternionf>>($"Hand{creality.Value}Rot");
				var userscale = world.LocalUser.FindOrCreateUserStream<SyncStream<Vector3f>>($"Hand{creality.Value}Scale");
				userpos.Value = entity.position.Value;
				userrot.Value = entity.rotation.Value;
				userscale.Value = entity.scale.Value;
			}
			else
			{
				if (userroot.Target.user.Target != null)
				{
					var temp = userroot.Target.user.Target;
					var userpos = temp.FindUserStream<SyncStream<Vector3f>>($"Hand{creality.Value}Pos");
					var userrot = temp.FindUserStream<SyncStream<Quaternionf>>($"Hand{creality.Value}Rot");
					var userscale = temp.FindUserStream<SyncStream<Vector3f>>($"Hand{creality.Value}Scale");

					try
					{
						Matrix4x4 value = Matrix4x4.CreateScale(userscale.Value.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(userrot.Value.ToSystemNumric()) * Matrix4x4.CreateTranslation(userpos.Value.ToSystemNumrics());
						entity.SetLocalTrans(value);
					}
					catch
					{

					}

				}
			}
		}

		public override void buildSyncObjs(bool newRefIds)
		{
			creality = new Sync<Creality>(this, newRefIds);
			userroot = new SyncRef<UserRoot>(this, newRefIds);
			posDriver = new Driver<Vector3f>(this, newRefIds);
			rotDriver = new Driver<Quaternionf>(this, newRefIds);
			scaleDriver = new Driver<Vector3f>(this, newRefIds);
		}

		public Hand(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public Hand()
		{
		}
	}
}