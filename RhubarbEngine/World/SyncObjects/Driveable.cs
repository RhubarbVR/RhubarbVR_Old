using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public interface Driveable : IWorldObject
	{
		bool isDriven { get; }
		NetPointer drivenFrom { get; }

		void killDrive();
	}
}
