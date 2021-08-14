using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbDataTypes
{
    [MessagePackObject]
    public struct NetPointer: IEquatable<NetPointer>
    {
        [Key(0)]
        public ulong id;

        public NetPointer(ulong _id)
        {
            id = _id;
        }

        public ulong getID()
        {
            return id;
        }
        public int getOwnerID()
        {
            return (int)((id >> 8)& 0xFF);
        }
        public static NetPointer BuildID(ulong position, byte user)
        {
            return new NetPointer(((position << 16)&0xFFFF0000) | ((((ulong)user) & 0xFF) << 8)|(position&0xFF));
        }

        public bool Equals(NetPointer other)
        {
            return other.id == id;
        }
    }
}
