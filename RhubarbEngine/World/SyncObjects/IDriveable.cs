using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public interface IDriveable : IWorldObject
	{
		bool IsDriven { get; }
		NetPointer DrivenFrom { get; }

		void KillDrive();
	}
}
