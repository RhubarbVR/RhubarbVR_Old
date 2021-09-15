using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public interface IDriveMember<T> : IValueSource<T>, IDriveable where T : IConvertible
	{
		void Drive(IDriver source);
		void ForceDrive(IDriver source);

	}
}
