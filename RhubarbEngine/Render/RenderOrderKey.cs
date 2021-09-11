using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace RhubarbEngine.Render
{
	public struct RenderOrderKey : IComparable<RenderOrderKey>, IComparable
	{
		public readonly ulong Value;

		public RenderOrderKey(ulong value)
		{
			Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RenderOrderKey Create(uint RenderOffsetint, float cameraDistance)
		{
			uint cameraDistanceInt = (uint)Math.Min(uint.MaxValue, (cameraDistance * 1000f));

			return new RenderOrderKey(
				((ulong)RenderOffsetint << 32) +
				cameraDistanceInt);
		}

		public int CompareTo(RenderOrderKey other)
		{
			return Value.CompareTo(other.Value);
		}

		int IComparable.CompareTo(object obj)
		{
			return Value.CompareTo(obj);
		}
	}
}
