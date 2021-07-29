using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Net
{
    public abstract class Peer
    {
        public virtual void Send(NetDataWriter val, ReliabilityLevel reliableOrdered)
        {
        }
    }
}
