using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbDataTypes
{
	public class RollBuffer
	{
		public byte[] array;

		public int length => array.Length;

		public int pos;

		public RollBuffer(int size)
		{
			array = new byte[size];
		}

		public void Push(byte e)
		{
			array[pos] = e;
			pos++;
			pos = pos % length;
		}

		public void Push(byte[] pushed)
		{
			foreach (var item in pushed)
			{
				Push(item);
			}
		}
	}
}
