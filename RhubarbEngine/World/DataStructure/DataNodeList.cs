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
    public class DataNodeList : IDataNode
    {

        private List<IDataNode> NodeGroup = new List<IDataNode>();
        public byte[] getByteArray()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(NodeGroup.Count);
                        for (int i = 0; i < NodeGroup.Count; i++)
                        {
                            byte[] value = NodeGroup[i].getByteArray();
                            writer.Write(Array.IndexOf(DatatNodeTools.dataNode, ((object)NodeGroup[i]).GetType()));
                            writer.Write(value.Count());
                            writer.Write(value);
                        }
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
            NodeGroup.Clear();
            using (var memStream = new MemoryStream())
            {
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(memStream))
                {
                    int Count = reader.ReadInt32();
                    for (int i = 0; i < Count; i++)
                    {
                        Type ty = DatatNodeTools.dataNode[reader.ReadInt32()];
                        int ValueCount = reader.ReadInt32();
                        byte[] value = reader.ReadBytes(ValueCount);
                        if (typeof(IDataNode).IsAssignableFrom(ty))
                        {
                            IDataNode valueobj = (IDataNode)Activator.CreateInstance(ty);
                            valueobj.setByteArray(value);
                            NodeGroup.Add(valueobj);
                        }
                        else
                        {
                            throw new Exception("Type is not valid when loading data.");
                        }
                    }
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

        public DataNodeList()
        {
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
