using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public interface IWorldObject
    {
		RefID ReferenceID { get; }

		World World { get; }

		IWorldObject Parent { get; }

		bool IsLocalObject { get; }

		bool IsPersistent { get; }

		bool IsRemoved { get; }

	}
}

