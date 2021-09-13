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

namespace RhubarbEngine.Components.Transform
{
	public enum LookAtPace
	{
		Root,
		Head,
		LeftController,
		RightController
	}

	[Category(new string[] { "Transform" })]
	public class LookAtUser : Component
	{
		public Driver<Quaternionf> driver;

		public Sync<Quaternionf> offset;

		public Sync<Vector3f> positionOffset;

		public Sync<LookAtPace> positionSource;

		public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Quaternionf>(this, newRefIds);
			offset = new Sync<Quaternionf>(this, newRefIds);
			offset.Value = Quaternionf.Identity;
			positionOffset = new Sync<Vector3f>(this, newRefIds);
			positionSource = new Sync<LookAtPace>(this, newRefIds);
			positionSource.Value = LookAtPace.Head;
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
				Vector3f? tagetPos;
				switch (positionSource.Value)
				{
					case LookAtPace.Root:
						tagetPos = World.LocalUser.userroot.Target?.Entity.GlobalPos();
						break;
					case LookAtPace.Head:
						tagetPos = World.LocalUser.userroot.Target?.Head.Target?.GlobalPos();
						break;
					case LookAtPace.LeftController:
						tagetPos = World.LocalUser.userroot.Target?.LeftHand.Target?.GlobalPos();
						break;
					case LookAtPace.RightController:
						tagetPos = World.LocalUser.userroot.Target?.RightHand.Target?.GlobalPos();
						break;
					default:
						tagetPos = null;
						break;
				}
				Vector3f tangent = (((tagetPos ?? Vector3f.AxisY) + positionOffset.Value) - Entity.GlobalPos());
				tangent.Normalize();
				Vector3f normal = Vector3f.AxisY;
				var newrot = Quaternionf.LookRotation(tangent, normal) * offset.Value;
				driver.Drivevalue = Entity.GlobalRotToLocal(newrot, false);
			}
			else
			{
				driver.Target = Entity.rotation;
			}
		}
		public LookAtUser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LookAtUser()
		{
		}
	}
}
