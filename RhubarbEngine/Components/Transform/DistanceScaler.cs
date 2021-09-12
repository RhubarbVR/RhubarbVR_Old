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

namespace RhubarbEngine.Components.Transform
{
	[Category(new string[] { "Transform" })]
	public class DistanceScaler : Component
	{

		public Driver<Vector3f> driver;

		public Sync<Vector3f> offset;

		public Sync<float> scale;

		public Sync<double> min;

		public Sync<double> max;

		public Sync<double> pow;

		public Sync<Vector3f> positionOffset;

		public Sync<LookAtPace> positionSource;

		public override void buildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Vector3f>(this, newRefIds);
			offset = new Sync<Vector3f>(this, newRefIds);
			positionOffset = new Sync<Vector3f>(this, newRefIds);
			positionSource = new Sync<LookAtPace>(this, newRefIds);
			positionSource.Value = LookAtPace.Head;
			scale = new Sync<float>(this, newRefIds);
			scale.Value = 1f;
			pow = new Sync<double>(this, newRefIds);
			pow.Value = 1;
			min = new Sync<double>(this, newRefIds);
			min.Value = 0.25f;
			max = new Sync<double>(this, newRefIds);
			max.Value = 2000f;
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
				Vector3f? tagetPos;
				switch (positionSource.Value)
				{
					case LookAtPace.Root:
						tagetPos = world.LocalUser.userroot.Target?.entity.GlobalPos();
						break;
					case LookAtPace.Head:
						tagetPos = world.LocalUser.userroot.Target?.Head.Target?.GlobalPos();
						break;
					case LookAtPace.LeftController:
						tagetPos = world.LocalUser.userroot.Target?.LeftHand.Target?.GlobalPos();
						break;
					case LookAtPace.RightController:
						tagetPos = world.LocalUser.userroot.Target?.RightHand.Target?.GlobalPos();
						break;
					default:
						tagetPos = null;
						break;
				}
				var dist = Math.Pow((entity.GlobalPos().Distance((tagetPos ?? Vector3f.Zero) + positionOffset.Value) * scale.Value), pow.Value);
				driver.Drivevalue = entity.GlobalScaleToLocal((new Vector3f(Math.Clamp(dist, min.Value, max.Value)) + offset.Value), false);
			}
			else
			{
				driver.Target = entity.scale;
			}
		}
		public DistanceScaler(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public DistanceScaler()
		{
		}
	}
}
