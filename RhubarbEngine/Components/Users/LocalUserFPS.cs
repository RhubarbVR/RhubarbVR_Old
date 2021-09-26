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
	public class LocalUserFPS : Component
	{
		public Driver<float> fps;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (fps.Linked)
			{
				fps.Drivevalue = World.worldManager.Engine.PlatformInfo.FrameRate;
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			fps = new Driver<float>(this, newRefIds);
		}

		public LocalUserFPS(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public LocalUserFPS()
		{
		}
	}
}