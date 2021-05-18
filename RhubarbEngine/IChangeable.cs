using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine
{
	public interface IChangeable
	{
		event Action<IChangeable> Changed;
	}
}
