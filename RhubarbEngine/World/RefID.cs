using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public class RefID
    {
        private readonly ulong id;

        public RefID(ulong _id)
        {
            id = _id;
        }

        public ulong getID()
        {
            return id;
        }

        public static RefID BuildID(ulong position, byte user)
        {
            ulong temp = (((position >> 8) << 32) | ((((ulong)user*2) & 511) << 16) | (position & 511));
            return new RefID((temp << 16)| (temp >> 16));
        }

    }
}
