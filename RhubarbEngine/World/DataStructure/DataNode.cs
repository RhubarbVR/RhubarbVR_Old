using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace RhubarbEngine.World.DataStructure
{
    public class DataNode<T>: IDataNode
    {
        public DataNode(T def = default(T))
        {
            Value = def;
        }
        public T Value;
        
        public byte[] getByteArray()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, Value);
                    return ms.ToArray();
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
        }

        public void setByteArray(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                Value = (T)binForm.Deserialize(memStream);
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
