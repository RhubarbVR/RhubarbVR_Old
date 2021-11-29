using RhubarbEngine.World;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Audio
{
	public interface IAudioSource : IWorldObject
	{
		public bool IsActive { get; }

		public int ChannelCount { get; }

        public event Action<byte[]> Update;

        public event Action Reload;
	}
}
