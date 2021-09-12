using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// 3D integer vector type. This is basically the same as Index3i but
	/// with .x.y.z member names. This makes code far more readable in many places.
	/// Unfortunately I can't see a way to do this w/o so much duplication...we could
	/// have .x/.y/.z accessors but that is much less efficient...
	/// </summary>
	[MessagePackObject]
	public struct Vector3i : IComparable<Vector3i>, IEquatable<Vector3i>, IConvertible
	{
		[Key(0)]
		public int x;
		[Key(1)]
		public int y;
		[Key(2)]
		public int z;

		public Vector3i(int f) { x = y = z = f; }
		public Vector3i(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
		public Vector3i(int[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }

		static public readonly Vector3i Zero = new Vector3i(0, 0, 0);
		static public readonly Vector3i One = new Vector3i(1, 1, 1);
		static public readonly Vector3i AxisX = new Vector3i(1, 0, 0);
		static public readonly Vector3i AxisY = new Vector3i(0, 1, 0);
		static public readonly Vector3i AxisZ = new Vector3i(0, 0, 1);

		public int this[int key]
		{
			get { return (key == 0) ? x : (key == 1) ? y : z; }
			set { if (key == 0) x = value; else if (key == 1) y = value; else z = value; }
		}

		public int[] array
		{
			get { return new int[] { x, y, z }; }
		}



		public void Set(Vector3i o)
		{
			x = o.x;
			y = o.y;
			z = o.z;
		}
		public void Set(int fX, int fY, int fZ)
		{
			x = fX;
			y = fY;
			z = fZ;
		}
		public void Add(Vector3i o)
		{
			x += o.x;
			y += o.y;
			z += o.z;
		}
		public void Subtract(Vector3i o)
		{
			x -= o.x;
			y -= o.y;
			z -= o.z;
		}
		public void Add(int s) { x += s; y += s; z += s; }


		public int LengthSquared { get { return x * x + y * y + z * z; } }


		public static Vector3i operator -(Vector3i v)
		{
			return new Vector3i(-v.x, -v.y, -v.z);
		}

		public static Vector3i operator *(int f, Vector3i v)
		{
			return new Vector3i(f * v.x, f * v.y, f * v.z);
		}
		public static Vector3i operator *(Vector3i v, int f)
		{
			return new Vector3i(f * v.x, f * v.y, f * v.z);
		}
		public static Vector3i operator /(Vector3i v, int f)
		{
			return new Vector3i(v.x / f, v.y / f, v.z / f);
		}
		public static Vector3i operator /(int f, Vector3i v)
		{
			return new Vector3i(f / v.x, f / v.y, f / v.z);
		}

		public static Vector3i operator *(Vector3i a, Vector3i b)
		{
			return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		public static Vector3i operator /(Vector3i a, Vector3i b)
		{
			return new Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);
		}


		public static Vector3i operator +(Vector3i v0, Vector3i v1)
		{
			return new Vector3i(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
		}
		public static Vector3i operator +(Vector3i v0, int f)
		{
			return new Vector3i(v0.x + f, v0.y + f, v0.z + f);
		}

		public static Vector3i operator -(Vector3i v0, Vector3i v1)
		{
			return new Vector3i(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
		}
		public static Vector3i operator -(Vector3i v0, int f)
		{
			return new Vector3i(v0.x - f, v0.y - f, v0.z - f);
		}




		public static bool operator ==(Vector3i a, Vector3i b)
		{
			return (a.x == b.x && a.y == b.y && a.z == b.z);
		}
		public static bool operator !=(Vector3i a, Vector3i b)
		{
			return (a.x != b.x || a.y != b.y || a.z != b.z);
		}
		public override bool Equals(object obj)
		{
			return this == (Vector3i)obj;
		}
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Vector3i other)
		{
			if (x != other.x)
				return x < other.x ? -1 : 1;
			else if (y != other.y)
				return y < other.y ? -1 : 1;
			else if (z != other.z)
				return z < other.z ? -1 : 1;
			return 0;
		}
		public bool Equals(Vector3i other)
		{
			return (x == other.x && y == other.y && z == other.z);
		}



		public override string ToString()
		{
			return string.Format("{0} {1} {2}", x, y, z);
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}



		// implicit cast between Index3i and Vector3i
		public static implicit operator Vector3i(Index3i v)
		{
			return new Vector3i(v.a, v.b, v.c);
		}
		public static implicit operator Index3i(Vector3i v)
		{
			return new Index3i(v.x, v.y, v.z);
		}

		// explicit cast to double/float vector types
		public static explicit operator Vector3i(Vector3f v)
		{
			return new Vector3i((int)v.x, (int)v.y, (int)v.z);
		}
		public static explicit operator Vector3f(Vector3i v)
		{
			return new Vector3f((float)v.x, (float)v.y, (float)v.z);
		}
		public static explicit operator Vector3i(Vector3d v)
		{
			return new Vector3i((int)v.x, (int)v.y, (int)v.z);
		}
		public static explicit operator Vector3d(Vector3i v)
		{
			return new Vector3d((double)v.x, (double)v.y, (double)v.z);
		}

	}
}
