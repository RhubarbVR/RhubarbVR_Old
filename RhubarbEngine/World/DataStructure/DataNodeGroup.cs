using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MessagePack;
namespace RhubarbEngine.World.DataStructure
{
    public class DataNodeGroup : IDataNode
    {

        private readonly Dictionary<string, IDataNode> _nodeGroup = new Dictionary<string, IDataNode>();
        public byte[] getByteArray()
        {
            try
            {
                var keyValuePairs = new Dictionary<byte[], byte[]>();
                foreach (var item in _nodeGroup.Keys)
                {
                    int typeint = (byte)Array.IndexOf(DatatNodeTools.dataNode, _nodeGroup[item].GetType());
                    if (typeint == -1 || typeint >= DatatNodeTools.dataNode.Count())
                    {
                        throw new Exception("Error not Assinded Type " + _nodeGroup[item].GetType().FullName);
                    }
                    var type = (byte)typeint;
                    var e = new List<byte>(Packer(item));
                    e.Insert(0, type);
                    keyValuePairs.Add(e.ToArray(), _nodeGroup[item].getByteArray());
                }
                var ps = new List<(byte[], byte[])>();
                foreach (var item in keyValuePairs.Keys)
                {
                    ps.Add((item, keyValuePairs[item]));
                }
                return MessagePackSerializer.Serialize(ps.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize. Group Reason: " + e.Message);
                return new byte[] { };
            }
        }
        //31 max hardpack values
        public static string[] HardPack = new string[] { "", "Value", "referenceID", "targetRefID", "list", "enabled", "updateOrder", "remderlayer", "parent", "_children", "name", "rotation", "scale", "position", "Type", "_components", "persistence" };

        public string UnPacker(byte[] inputeval)
        {
            try
            {
                var ascii = new ASCIIEncoding();
                var hardPackval = inputeval[0] - 1;
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

        public byte[] Packer(string inputeval)
        {
            try
            {
                var ascii = new ASCIIEncoding();
                var val = inputeval;
                var hardpackvalue = Array.IndexOf(HardPack, inputeval);
                if (hardpackvalue > 0)
                {
                    hardpackvalue++;
                    val = ((char)hardpackvalue).ToString();
                }
                var bytes = new byte[1];
                var count = ascii.GetByteCount(val);
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
        public IDataNode GetValue(string key)
        {
            return _nodeGroup.GetValueOrDefault(key);
        }
        public void SetValue(string key, IDataNode obj)
        {
            _nodeGroup.Add(key, obj);
        }
        public void setByteArray(byte[] arrBytes)
        {
            _nodeGroup.Clear();
            try
            {
                var temp = MessagePackSerializer.Deserialize<(byte[], byte[])[]>(arrBytes);
                var data = new Dictionary<byte[], byte[]>();
                for (var i = 0; i < temp.Count(); i++)
                {
                    data.Add(temp[i].Item1, temp[i].Item2);
                }
                foreach (var item in data.Keys)
                {

                    var type = DatatNodeTools.dataNode[item[0]];
                    var obj = (IDataNode)Activator.CreateInstance(type);
                    obj.setByteArray(data[item]);
                    var key = new List<byte>(item);
                    key.RemoveAt(0);
                    var keyval = UnPacker(key.ToArray());
                    _nodeGroup.Add(keyval, obj);

                }
            }
            catch (Exception e)
            {
                Logger.Log("Failed to deserialize. Group Reason: " + e.Message);
            }
        }


        public DataNodeGroup()
        {
        }


        public DataNodeGroup(byte[] data)
        {
            setByteArray(data);
        }
    }
}
