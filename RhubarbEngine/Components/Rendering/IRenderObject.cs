using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Rendering
{
	public enum RenderFrequency
	{
		OneToOne = 0,
		Half = 1,
        Forth = 2,
        Eighth = 3,
        Sixteenth = 4,
	}

	public interface IRenderObject
	{
		RenderFrequency RenderFrac { get; }

		bool Threaded { get; }

		void Render();
	}
}
