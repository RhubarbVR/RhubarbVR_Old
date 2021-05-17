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
    public class DataNodeGroup : IDataNode
    {

        private Dictionary<string, IDataNode> NodeGroup = new Dictionary<string, IDataNode>();
        public byte[] getByteArray()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(NodeGroup.Count);
                        string[] keys = new string[] { };
                        IDataNode[] values = new IDataNode[] { };
                        Array.Resize(ref keys, NodeGroup.Count);
                        Array.Resize(ref values, NodeGroup.Count);
                        NodeGroup.Keys.CopyTo(keys,0);
                        NodeGroup.Values.CopyTo(values,0);
                        for (int i = 0; i < NodeGroup.Count; i++)
                        {
                            writer.Write(keys[i]);
                            byte[] value = values[i].getByteArray();
                            writer.Write(((object)values[i]).GetType().FullName);
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
                        string key = reader.ReadString();
                        Type ty = Type.GetType(reader.ReadString());
                        int ValueCount = reader.ReadInt32();
                        byte[] value = reader.ReadBytes(ValueCount);
                        if (typeof(IDataNode).IsAssignableFrom(ty))
                        {
                            IDataNode valueobj = (IDataNode)Activator.CreateInstance(ty);
                            valueobj.setByteArray(value);
                            NodeGroup.Add(key, valueobj);
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

        public DataNodeGroup()
        {
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
