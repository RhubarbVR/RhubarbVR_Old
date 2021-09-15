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
	public class Spinner : Component
	{
		public Driver<Quaternionf> driver;

		public Sync<Vector3f> speed;

		public Sync<Quaternionf> offset;

		public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Quaternionf>(this, newRefIds);
            speed = new Sync<Vector3f>(this, newRefIds)
            {
                Value = new Vector3f(1f, 0f, 0f)
            };
            offset = new Sync<Quaternionf>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			var deltaSeconds = (float)World.worldManager.engine.platformInfo.deltaSeconds;
			if (driver.Linked)
			{
				var newval = Entity.LocalTrans() * Matrix4x4.CreateFromQuaternion(offset.Value.ToSystemNumric()) * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(speed.Value.x * deltaSeconds, speed.Value.y * deltaSeconds, speed.Value.z * deltaSeconds));
				Matrix4x4.Decompose(newval, out _, out var newrotation, out _);
				driver.Drivevalue = new Quaternionf(newrotation.X, newrotation.Y, newrotation.Z, newrotation.W);
			}
			else
			{
				driver.Target = Entity.rotation;
			}
		}
		public Spinner(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public Spinner()
		{
		}
	}
}
