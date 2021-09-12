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

namespace RhubarbEngine
{

}

namespace RhubarbEngine.Components.Users
{

	[Category(new string[] { "Users" })]
	public class SimpleSpawn : Component
	{

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (!world.userLoaded)
			{
				return;
			}
			if (world.LocalUser.userroot.Target == null)
			{
				Entity rootent = world.RootEntity.AddChild();
				rootent.name.Value = $"{world.LocalUser.username.Value} (ID:{world.LocalUser.referenceID.id.ToHexString()})";
				rootent.persistence.Value = false;
				rootent.Manager = world.LocalUser;
				UserRoot userRoot = rootent.AttachComponent<UserRoot>();
				userRoot.user.Target = world.LocalUser;
				world.LocalUser.userroot.Target = userRoot;
				Entity head = rootent.AddChild("Head");
				head.AttachComponent<Head>().userroot.Target = userRoot;
				var grabHolder = head.AddChild("GrabHolder").AttachComponent<GrabbableHolder>();
				grabHolder.initializeGrabHolder(InteractionSource.HeadLaser);
				userRoot.Head.Target = head;
				Entity left = rootent.AddChild("Left hand");
				Entity right = rootent.AddChild("Right hand");


				userRoot.LeftHand.Target = left;
				userRoot.RightHand.Target = right;
				Hand leftcomp = left.AttachComponent<Hand>();
				leftcomp.userroot.Target = userRoot;
				leftcomp.creality.Value = Input.Creality.Left;
				Hand rightcomp = right.AttachComponent<Hand>();
				rightcomp.creality.Value = Input.Creality.Right;
				rightcomp.userroot.Target = userRoot;


				Entity obj = world.worldManager.AddMesh<ArrowMesh>(left);
				Entity obj2 = world.worldManager.AddMesh<ArrowMesh>(right);
				Entity obj3 = world.worldManager.AddMesh<ArrowMesh>(head);

				var ileft = left.AttachComponent<GrabbableHolder>();
				var iright = right.AttachComponent<GrabbableHolder>();
				ileft.initializeGrabHolder(InteractionSource.LeftLaser);
				iright.initializeGrabHolder(InteractionSource.RightLaser);

				obj3.position.Value = new Vector3f(0f, 0f, 0.5f);
				obj.scale.Value = new Vector3f(0.2f);
				obj2.scale.Value = new Vector3f(0.2f);
				obj.rotation.Value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
				obj2.rotation.Value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
				logger.Log("SpawnedUser");
			}
		}

		public override void buildSyncObjs(bool newRefIds)
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