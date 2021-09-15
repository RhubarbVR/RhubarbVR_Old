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
using RhubarbEngine.Input;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;


namespace RhubarbEngine.Components.Users
{

	[Category(new string[] { "Users" })]
	public class SimpleSpawn : Component
	{

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (!World.userLoaded)
			{
				return;
			}
			if (World.LocalUser.userroot.Target == null)
			{
				var rootent = World.RootEntity.AddChild();
				rootent.name.Value = $"{World.LocalUser.username.Value} (ID:{World.LocalUser.ReferenceID.id.ToHexString()})";
				rootent.persistence.Value = false;
				rootent.Manager = World.LocalUser;
				var userRoot = rootent.AttachComponent<UserRoot>();
				userRoot.user.Target = World.LocalUser;
				World.LocalUser.userroot.Target = userRoot;
				var head = rootent.AddChild("Head");
				head.AttachComponent<Head>().userroot.Target = userRoot;
				var grabHolder = head.AddChild("GrabHolder").AttachComponent<GrabbableHolder>();
				grabHolder.InitializeGrabHolder(InteractionSource.HeadLaser);
				userRoot.Head.Target = head;
				var left = rootent.AddChild("Left hand");
				var right = rootent.AddChild("Right hand");


				userRoot.LeftHand.Target = left;
				userRoot.RightHand.Target = right;
				var leftcomp = left.AttachComponent<Hand>();
				leftcomp.userroot.Target = userRoot;
				leftcomp.creality.Value = RhubarbEngine.Input.Creality.Left;
				var rightcomp = right.AttachComponent<Hand>();
				rightcomp.creality.Value = RhubarbEngine.Input.Creality.Right;
				rightcomp.userroot.Target = userRoot;


				var obj = Managers.WorldManager.AddMesh<ArrowMesh>(left);
				var obj2 = Managers.WorldManager.AddMesh<ArrowMesh>(right);
				var obj3 = Managers.WorldManager.AddMesh<ArrowMesh>(head);

				var ileft = left.AttachComponent<GrabbableHolder>();
				var iright = right.AttachComponent<GrabbableHolder>();
				ileft.InitializeGrabHolder(InteractionSource.LeftLaser);
				iright.InitializeGrabHolder(InteractionSource.RightLaser);

				obj3.position.Value = new Vector3f(0f, 0f, 0.5f);
				obj.scale.Value = new Vector3f(0.2f);
				obj2.scale.Value = new Vector3f(0.2f);
				obj.rotation.Value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
				obj2.rotation.Value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
				Logger.Log("SpawnedUser");
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
		}

		public SimpleSpawn(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SimpleSpawn()
		{
		}
	}
}