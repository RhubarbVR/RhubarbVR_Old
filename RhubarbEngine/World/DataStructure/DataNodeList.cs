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

		private readonly List<IDataNode> _nodeGroup = new();
		public byte[] GetByteArray()
		{
			try
			{
				var keyValuePairs = new List<byte[]>();
				for (var index = 0; index < _nodeGroup.Count; index++)
				{
					var item = _nodeGroup[index];
					if (item == null)
					{
						Console.WriteLine(_nodeGroup[0].GetType().ToString() + _nodeGroup[1].GetType().ToString());
						Console.WriteLine("okay" + index.ToString() + " hi: " + _nodeGroup.Count.ToString());
					}
					var type = (byte)Array.IndexOf(DatatNodeTools.dataNode, item.GetType());
					var value = new List<byte>(item.GetByteArray());
					value.Insert(0, type);
					keyValuePairs.Add(value.ToArray());
				}
				return MessagePackSerializer.Serialize(keyValuePairs);
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to serialize list. Reason: " + e.Message);
				throw;
			}
		}

		public IEnumerator<IDataNode> GetEnumerator()
		{
			for (var i = 0; i < _nodeGroup.Count; i++)
			{
				yield return this[i];
			}
		}

		IDataNode this[int i]
		{
			get
			{
				return _nodeGroup[i];
			}
			set
			{
				_nodeGroup[i] = value;
			}
		}

		public void Add(IDataNode val)
		{
			_nodeGroup.Add(val);
		}
		public void SetByteArray(byte[] arrBytes)
		{
			_nodeGroup.Clear();
			try
			{
				var keyValuePairs = MessagePackSerializer.Deserialize<List<byte[]>>(arrBytes);

				foreach (var item in keyValuePairs)
				{
					var type = DatatNodeTools.dataNode[item[0]];
					var obj = (IDataNode)Activator.CreateInstance(type);
					var val = new List<byte>(item);
					val.RemoveAt(0);
					obj.SetByteArray(val.ToArray());
					_nodeGroup.Add(obj);
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
			SetByteArray(data);
		}
	}
}
