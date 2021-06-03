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
                            byte[] keyBytes = packer(keys[i]);
                            writer.Write(keyBytes.Count());
                            writer.Write(keyBytes);
                            byte[] value = values[i].getByteArray();
                            writer.Write(Array.IndexOf(DatatNodeTools.dataNode, ((object)values[i]).GetType()));
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
                return new byte[]{ };
            }
        }
        //31 max hardpack values
        public static string[] HardPack = new String[] { "","Value", "referenceID", "targetRefID", "list", "enabled", "updateOrder" , "remderlayer", "parent", "_children", "Entity", "name", "rotation", "scale", "position", "Type" };

        public string unPacker(byte[] inputeval)
        {
            try
            {
            ASCIIEncoding ascii = new ASCIIEncoding();
            int hardPackval = ((int)(inputeval[0])) - 1;
                if (hardPackval < HardPack.Length)
                {
                    return HardPack[hardPackval];
                }
                else
                {
                    return ascii.GetString(inputeval);
                }
            }
            catch (Exception e)
            {
                throw new Exception("UnPack Failed with Bytes:" + inputeval.ToString() + " Error:" + e.Message);
            }
        }

        public byte[] packer(String inputeval)
        {
            try
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                string val = inputeval;
                int hardpackvalue = Array.IndexOf(HardPack, inputeval);
                if (hardpackvalue > 0)
                {
                    hardpackvalue++;
                    val = ((char)hardpackvalue).ToString();
                }
                byte[] bytes = new byte[1];
                int count = ascii.GetByteCount(val);
                if (count >= bytes.Length)
                {
                    Array.Resize(ref bytes, count);
                }
                ascii.GetBytes(val, 0, val.Length, bytes, 0);
                return bytes;
            }
            catch (Exception e)
            {
                throw new Exception("Pack Failed with string:" + inputeval + " Error:" + e.Message);
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
                        string key = unPacker(reader.ReadBytes(reader.ReadInt32()));
                        Type ty = DatatNodeTools.dataNode[reader.ReadInt32()];
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
