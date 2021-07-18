using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
namespace RhubarbEngine.World.Net
{
    public class NetData
    {
        public ReliabilityLevel reliabilityLevel;

        public DataNodeGroup data;

        public ulong id;

        public NetData(ReliabilityLevel _reliabilityLevel, DataNodeGroup _data, ulong _id)
        {
            reliabilityLevel = _reliabilityLevel;
            data = _data;
            id = _id;
        }

    }
}
