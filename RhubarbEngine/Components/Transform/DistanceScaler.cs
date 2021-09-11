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
			positionSource.value = LookAtPace.Head;
			scale = new Sync<float>(this, newRefIds);
			scale.value = 1f;
			pow = new Sync<double>(this, newRefIds);
			pow.value = 1;
			min = new Sync<double>(this, newRefIds);
			min.value = 0.25f;
			max = new Sync<double>(this, newRefIds);
			max.value = 2000f;
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
				Vector3f? tagetPos;
				switch (positionSource.value)
				{
					case LookAtPace.Root:
						tagetPos = world.localUser.userroot.target?.entity.globalPos();
						break;
					case LookAtPace.Head:
						tagetPos = world.localUser.userroot.target?.Head.target?.globalPos();
						break;
					case LookAtPace.LeftController:
						tagetPos = world.localUser.userroot.target?.LeftHand.target?.globalPos();
						break;
					case LookAtPace.RightController:
						tagetPos = world.localUser.userroot.target?.RightHand.target?.globalPos();
						break;
					default:
						tagetPos = null;
						break;
				}
				var dist = Math.Pow((entity.globalPos().Distance((tagetPos ?? Vector3f.Zero) + positionOffset.value) * scale.value), pow.value);
				driver.Drivevalue = entity.GlobalScaleToLocal((new Vector3f(Math.Clamp(dist, min.value, max.value)) + offset.value), false);
			}
			else
			{
				driver.target = entity.scale;
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
