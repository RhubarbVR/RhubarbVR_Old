using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MessagePack;
namespace RhubarbEngine.World.DataStructure
{
    public class DataNodeGroup : IDataNode
    {

        private Dictionary<string, IDataNode> NodeGroup = new Dictionary<string, IDataNode>();
        public byte[] getByteArray()
        {
            try
            {
                Dictionary<byte[], byte[]> keyValuePairs = new Dictionary<byte[], byte[]>();
                foreach (var item in NodeGroup.Keys)
                {

                    byte type = (byte)Array.IndexOf(DatatNodeTools.dataNode, NodeGroup[item].GetType());
                    List<byte> e = new List<byte>(packer(item));
                    e.Insert(0,type);
                    keyValuePairs.Add(e.ToArray(), NodeGroup[item].getByteArray());
                }
                return MessagePackSerializer.Serialize(keyValuePairs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize. Group Reason: " + e.Message);
                return new byte[]{ };
            }
        }
        //31 max hardpack values
        public static string[] HardPack = new String[] { "","Value", "referenceID", "targetRefID", "list", "enabled", "updateOrder" , "remderlayer", "parent", "_children", "name", "rotation", "scale", "position", "Type", "_components", "persistence" };

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
            try {
                Dictionary<byte[], byte[]>  data = MessagePackSerializer.Deserialize<Dictionary<byte[], byte[]>>(arrBytes);
                foreach (var item in data.Keys)
                {
                    Type type = DatatNodeTools.dataNode[item[0]];
                    IDataNode obj = (IDataNode)Activator.CreateInstance(type);
                    obj.setByteArray(data[item]);
                    List<byte> key = new List<byte>(item);
                    key.RemoveAt(0);
                    string keyval = unPacker(key.ToArray());
                    NodeGroup.Add(keyval, obj);
                }
            }
            catch (Exception e)
            {
                Logger.Log("Failed to deserialize. Group Reason: " + e.Message);
            }
        }
        public void Deserialize(NetDataReader reader)
        {
            setByteArray(reader.GetBytesWithLength());
        }
        public void Serialize(NetDataWriter writer)
        {
            byte[] value = getByteArray();
            writer.PutBytesWithLength(value);
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
