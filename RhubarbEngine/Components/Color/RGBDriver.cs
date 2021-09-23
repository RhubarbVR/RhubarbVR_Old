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
	public class RGBDriver : Component
	{
		public Driver<Colorf> driver;

		public Sync<ColorHSV> Value;

		public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<Colorf>(this, newRefIds);
			Value = new Sync<ColorHSV>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
				driver.Drivevalue = Value.Value;
			}
		}

		public RGBDriver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}

		public RGBDriver()
		{
		}
	}
}
