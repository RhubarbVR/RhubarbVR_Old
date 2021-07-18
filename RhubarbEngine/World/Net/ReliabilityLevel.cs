using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Net
{
    public enum ReliabilityLevel: byte
    {
        Unreliable,
        LatestOnly,
        Reliable
    }
}
