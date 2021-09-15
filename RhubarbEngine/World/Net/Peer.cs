using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Net
{
	public abstract class Peer
	{
		public string UserUUID { get; }
		public virtual void Send(byte[] val, ReliabilityLevel reliableOrdered)
		{
		}
	}
}
