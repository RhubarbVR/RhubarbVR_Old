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
    public class DataNodeList : IDataNode
    {

        private List<IDataNode> NodeGroup = new List<IDataNode>();
        public byte[] getByteArray()
        {
            try
            {
                List<byte[]> keyValuePairs = new List<byte[]>();
                for (int index = 0; index < NodeGroup.Count; index++)
                {
                    var item = NodeGroup[index];
                    if(item == null)
                    {
                        Console.WriteLine(NodeGroup[0].GetType().ToString() + NodeGroup[1].GetType().ToString());
                        Console.WriteLine("okay"+index.ToString()+" hi: "+NodeGroup.Count.ToString());
                    }
                    byte type = (byte)Array.IndexOf(DatatNodeTools.dataNode, item.GetType());
                    List<byte> value = new List<byte>(item.getByteArray());
                    value.Insert(0, type);
                    keyValuePairs.Add(value.ToArray());
                }
                return MessagePackSerializer.Serialize(keyValuePairs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize list. Reason: " + e.Message );
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
            try
            {
                List<byte[]> keyValuePairs = MessagePackSerializer.Deserialize<List< byte[]>>(arrBytes);

                foreach (var item in keyValuePairs)
                {
                    Type type = DatatNodeTools.dataNode[item[0]];
                    IDataNode obj = (IDataNode)Activator.CreateInstance(type);
                    List<byte> val = new List<byte>(item);
                    val.RemoveAt(0);
                    obj.setByteArray(val.ToArray());
                    NodeGroup.Add(obj);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize list. Reason: " + e.Message);
                throw;
            }
        }

        public DataNodeList()
        {
        }


        public DataNodeList(byte[] data)
        {
            setByteArray(data);
        }
    }
}
