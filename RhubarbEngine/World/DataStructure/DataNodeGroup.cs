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
    [Serializable()]
    public class DataNodeGroup : IDataNode
    {

        private Dictionary<string, IDataNode> NodeGroup;
        public byte[] getByteArray()
        {
            //Need to change to more secure method like binarywriter 
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, NodeGroup);
                    return ms.ToArray();
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
        }

        public IDataNode getValue(string key)
        {
            return NodeGroup.GetValueOrDefault(key);
        }
        public void setValue(string key, IDataNode obj)
        {
            NodeGroup.Add(key, obj);
        }
        public void setByteArray(byte[] arrBytes)
        {
            //Need to change to more secure method like binarywriter 
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                NodeGroup = (Dictionary<string, IDataNode>)binForm.Deserialize(memStream);
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

        public DataNodeGroup()
        {
            NodeGroup = new Dictionary<string, IDataNode>();
        }

        public DataNodeGroup(NetDataReader reader)
        {
            Deserialize(reader);
        }

        public DataNodeGroup(byte[] data)
        {
            setByteArray(data);
        }
    }
}
