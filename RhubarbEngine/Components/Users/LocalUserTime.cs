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

namespace RhubarbEngine.Components.Users
{
	[Category(new string[] { "Users" })]
	public class LocalUserTime : Component
	{
		public Driver<DateTime> currentTime;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (currentTime.Linked)
			{
				currentTime.Drivevalue = DateTime.UtcNow;
			}
		}

		public override void buildSyncObjs(bool newRefIds)
		{
			currentTime = new Driver<DateTime>(this, newRefIds);
		}

		public LocalUserTime(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LocalUserTime()
		{
		}
	}
}