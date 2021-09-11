using MessagePack;

using System;

namespace g3
{
	[MessagePackObject]
	public struct Vector2u : IComparable<Vector2u>, IEquatable<Vector2u>, IConvertible
	{
		[Key(0)]
		public uint x;
		[Key(1)]
		public uint y;

		public Vector2u(uint f) { x = y = f; }
		public Vector2u(uint x, uint y) { this.x = x; this.y = y; }
		public Vector2u(uint[] v2) { x = v2[0]; y = v2[1]; }
		[IgnoreMember]
		static public readonly Vector2u Zero = new Vector2u(0, 0);
		[IgnoreMember]
		static public readonly Vector2u One = new Vector2u(1, 1);
		[IgnoreMember]
		static public readonly Vector2u AxisX = new Vector2u(1, 0);
		[IgnoreMember]
		static public readonly Vector2u AxisY = new Vector2u(0, 1);
		[IgnoreMember]
		public uint this[uint key]
		{
			get { return (key == 0) ? x : y; }
			set { if (key == 0) x = value; else y = value; }
		}
		[IgnoreMember]
		public uint[] array
		{
			get { return new uint[] { x, y }; }
		}

		public void Add(uint s) { x += s; y += s; }

		[IgnoreMember]
		public uint LengthSquared { get { return x * x + y * y; } }


		public static Vector2u operator -(Vector2u v)
		{
			return new Vector2u((uint)-(int)v.x, (uint)-(int)v.y);
		}

		public static Vector2u operator *(uint f, Vector2u v)
		{
			return new Vector2u(f * v.x, f * v.y);
		}
		public static Vector2u operator *(Vector2u v, uint f)
		{
			return new Vector2u(f * v.x, f * v.y);
		}
		public static Vector2u operator /(Vector2u v, uint f)
		{
			return new Vector2u(v.x / f, v.y / f);
		}
		public static Vector2u operator /(uint f, Vector2u v)
		{
			return new Vector2u(f / v.x, f / v.y);
		}

		public static Vector2u operator *(Vector2u a, Vector2u b)
		{
			return new Vector2u(a.x * b.x, a.y * b.y);
		}
		public static Vector2u operator /(Vector2u a, Vector2u b)
		{
			return new Vector2u(a.x / b.x, a.y / b.y);
		}


		public static Vector2u operator +(Vector2u v0, Vector2u v1)
		{
			return new Vector2u(v0.x + v1.x, v0.y + v1.y);
		}
		public static Vector2u operator +(Vector2u v0, uint f)
		{
			return new Vector2u(v0.x + f, v0.y + f);
		}

		public static Vector2u operator -(Vector2u v0, Vector2u v1)
		{
			return new Vector2u(v0.x - v1.x, v0.y - v1.y);
		}
		public static Vector2u operator -(Vector2u v0, uint f)
		{
			return new Vector2u(v0.x - f, v0.y - f);
		}



		public static bool operator ==(Vector2u a, Vector2u b)
		{
			return (a.x == b.x && a.y == b.y);
		}
		public static bool operator !=(Vector2u a, Vector2u b)
		{
			return (a.x != b.x || a.y != b.y);
		}
		public override bool Equals(object obj)
		{
			return this == (Vector2u)obj;
		}
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				return hash;
			}

		}
		public int CompareTo(Vector2u other)
		{
			if (x != other.x)
				return x < other.x ? -1 : 1;
			else if (y != other.y)
				return y < other.y ? -1 : 1;
			return 0;
		}
		public bool Equals(Vector2u other)
		{
			return (x == other.x && y == other.y);
		}



		public override string ToString()
		{
			return string.Format("{0} {1}", x, y);
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
	}



}
