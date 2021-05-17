using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseR
{
    [Serializable()]
    public struct RefID
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
            return new RefID(((position << 16)&0xFFFF0000) | ((((ulong)user) & 0xFF) << 8)|(position&0xFF));
        }

    }
}
