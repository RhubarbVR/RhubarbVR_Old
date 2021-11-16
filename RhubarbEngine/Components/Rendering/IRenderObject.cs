using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Rendering
{
	public enum RenderFrequency
	{
		OneToOne,
		Half,
		Eighth,
        Sixteenth,
	}

	public interface IRenderObject
	{
		RenderFrequency RenderFrac { get; }

		bool Threaded { get; }

		void Render();
	}
}
