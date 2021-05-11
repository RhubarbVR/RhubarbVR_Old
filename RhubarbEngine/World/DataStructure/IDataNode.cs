using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;

namespace RhubarbEngine.World.DataStructure
{
    public interface IDataNode : INetSerializable
    {
        byte[] getByteArray();

        void setByteArray(byte[] array);

    }
}
