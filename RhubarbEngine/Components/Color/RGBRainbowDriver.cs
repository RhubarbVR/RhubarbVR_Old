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

namespace RhubarbEngine.Components.Color
{
	[Category(new string[] { "Color" })]
	public class RGBRainbowDriver : Component
	{
		public Driver<Colorf> driver;

		public Sync<float> speed;
		
        public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Colorf>(this, newRefIds);
            speed = new Sync<float>(this, newRefIds)
            {
                Value = 50f
            };
        }

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			var deltaSeconds = (float)World.worldManager.engine.PlatformInfo.DeltaSeconds;
			if (driver.Linked)
			{
				ColorHSV color = driver.Drivevalue;
				driver.Drivevalue = color.UpdateHue(deltaSeconds * speed.Value);
			}
		}

		public RGBRainbowDriver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}

		public RGBRainbowDriver()
		{
		}
	}
}
