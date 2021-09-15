using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public interface IValueSource<T> : IChangeable, IWorldObject where T : IConvertible
	{
		T Value { get; set; }

	}
}
