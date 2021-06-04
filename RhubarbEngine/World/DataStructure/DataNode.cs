using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using RhubarbDataTypes;

namespace RhubarbEngine.World.DataStructure
{
    public class DataNode<T>: IDataNode
    {
        public DataNode(T def = default(T))
        {
            Value = def;
        }

        public DataNode()
        {
            Value = default(T);
        }
        public T Value;
        
        public byte[] getByteArray()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        RhubarbIO.Serialize<T>(writer, Value);
                    }
                    byte[] retunval = ms.ToArray();
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
                throw new Exception("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
            }
        }

        public void setByteArray(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(memStream))
                {
                    Value = (T)RhubarbIO.DeSerialize<T>(reader);
                }
            }

        }

        public void Deserialize(NetDataReader reader)
        {
            setByteArray(reader.GetBytesWithLength());
        }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(getByteArray());
        }

    }
}
