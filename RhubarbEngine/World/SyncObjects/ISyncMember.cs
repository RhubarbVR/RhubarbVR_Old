using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using LiteNetLib;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
    public interface ISyncMember
    {
        void ReceiveData(DataNodeGroup data, Peer peer);
    }
}
