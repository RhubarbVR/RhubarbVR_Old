using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using RhubarbDataTypes;
using MessagePack;

namespace RhubarbEngine.World.DataStructure
{
	public class DataNode<T> : IDataNode
	{
		public DataNode(T def = default)
		{
			Value = def;
		}

		public DataNode()
		{
			Value = default;
		}
		public T Value;

		public byte[] GetByteArray()
		{
			try
			{
				return MessagePackSerializer.Serialize(Value);
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
				throw new Exception("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
			}
		}

		public void SetByteArray(byte[] arrBytes)
		{
			try
			{
				Value = MessagePackSerializer.Deserialize<T>(arrBytes);
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
				throw new Exception("Failed to serialize. Reason: " + e.Message + " Type: " + Value.GetType().FullName);
			}
		}

	}
}
