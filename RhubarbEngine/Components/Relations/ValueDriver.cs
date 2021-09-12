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

namespace RhubarbEngine.Components.Relations
{
	[Category(new string[] { "Relations" })]
	public class ValueDriver<T> : Component where T : IConvertible
	{
		public Driver<T> driver;

		public SyncRef<ValueSource<T>> source;

		public override void buildSyncObjs(bool newRefIds)
		{
			driver = new Driver<T>(this, newRefIds);
			source = new SyncRef<ValueSource<T>>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (driver.Linked)
			{
				if (source.Target != null)
				{
					driver.Drivevalue = source.Target.Value;
				}
			}
		}
		public ValueDriver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ValueDriver()
		{
		}
	}
}
