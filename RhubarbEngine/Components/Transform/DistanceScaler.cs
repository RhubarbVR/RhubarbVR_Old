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

		public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Vector3f>(this, newRefIds);
			offset = new Sync<Vector3f>(this, newRefIds);
			positionOffset = new Sync<Vector3f>(this, newRefIds);
            positionSource = new Sync<LookAtPace>(this, newRefIds)
            {
                Value = LookAtPace.Head
            };
            scale = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
            pow = new Sync<double>(this, newRefIds)
            {
                Value = 1
            };
            min = new Sync<double>(this, newRefIds)
            {
                Value = 0.25f
            };
            max = new Sync<double>(this, newRefIds)
            {
                Value = 2000f
            };
        }

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
                var tagetPos = positionSource.Value switch
                {
                    LookAtPace.Root => World.LocalUser.userroot.Target?.Entity.GlobalPos(),
                    LookAtPace.Head => World.LocalUser.userroot.Target?.Head.Target?.GlobalPos(),
                    LookAtPace.LeftController => World.LocalUser.userroot.Target?.LeftHand.Target?.GlobalPos(),
                    LookAtPace.RightController => World.LocalUser.userroot.Target?.RightHand.Target?.GlobalPos(),
                    _ => null,
                };
                var dist = Math.Pow(Entity.GlobalPos().Distance((tagetPos ?? Vector3f.Zero) + positionOffset.Value) * scale.Value, pow.Value);
				driver.Drivevalue = Entity.GlobalScaleToLocal(new Vector3f(Math.Clamp(dist, min.Value, max.Value)) + offset.Value, false);
			}
			else
			{
				driver.Target = Entity.scale;
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
