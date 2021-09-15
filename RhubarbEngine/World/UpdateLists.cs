using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.Components.Audio;
using RhubarbEngine.Components.Rendering;

namespace RhubarbEngine.World
{
	public class UpdateLists
	{
		public SynchronizedCollection<AudioOutput> audioOutputs = new ();
		public SynchronizedCollection<IRenderObject> trenderObject = new ();
		public SynchronizedCollection<IRenderObject> renderObject = new ();



	}


}
