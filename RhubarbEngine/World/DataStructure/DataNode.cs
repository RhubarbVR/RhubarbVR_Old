using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using BaseR;

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
                        writer.Write(typeof(T).FullName);
                        RhubarbIO.Serialize<T>(writer, Value);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
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
                    string typestr = reader.ReadString();
                    if(typestr != typeof(T).FullName)
                    {
                        throw new Exception("Type not the same as old type");
                    }
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
