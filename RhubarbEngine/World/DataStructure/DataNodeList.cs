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
    public class DataNodeList : IDataNode
    {

        private List<IDataNode> NodeGroup;
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

        public IEnumerator<IDataNode> GetEnumerator()
        {
            for (int i = 0; i < NodeGroup.Count; i++)
            {
                yield return this[i];
            }
        }

        IDataNode this[int i] {
            get
            {
                return NodeGroup[i];
            }
            set
            {
                NodeGroup[i] = value;
            }
        }

        public void Add(IDataNode val)
        {
            NodeGroup.Add(val);
        }
        public void setByteArray(byte[] arrBytes)
        {
            //Need to change to more secure method like binarywriter 
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                NodeGroup = (List<IDataNode>)binForm.Deserialize(memStream);
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

        public DataNodeList()
        {
            NodeGroup = new List<IDataNode>();
        }

        public DataNodeList(NetDataReader reader)
        {
            Deserialize(reader);
        }

        public DataNodeList(byte[] data)
        {
            setByteArray(data);
        }
    }
}
