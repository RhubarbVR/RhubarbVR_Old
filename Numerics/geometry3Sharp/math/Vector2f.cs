using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using System.Numerics;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector2f : IComparable<Vector2f>, IEquatable<Vector2f>, IConvertible
	{
		[Key(0)]
		public float x;
		[Key(1)]
		public float y;

		public Vector2f(float f) { x = y = f; }
		public Vector2f(float x, float y) { this.x = x; this.y = y; }
		public Vector2f(float[] v2) { x = v2[0]; y = v2[1]; }
		public Vector2f(double f) { x = y = (float)f; }
		public Vector2f(double x, double y) { this.x = (float)x; this.y = (float)y; }
		public Vector2f(double[] v2) { x = (float)v2[0]; y = (float)v2[1]; }
		public Vector2f(Vector2f copy) { x = copy[0]; y = copy[1]; }
		public Vector2f(Vector2d copy) { x = (float)copy[0]; y = (float)copy[1]; }

		[IgnoreMember]
		static public readonly Vector2f Zero = new Vector2f(0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector2f One = new Vector2f(1.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector2f AxisX = new Vector2f(1.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector2f AxisY = new Vector2f(0.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector2f MaxValue = new Vector2f(float.MaxValue, float.MaxValue);
		[IgnoreMember]
		static public readonly Vector2f MinValue = new Vector2f(float.MinValue, float.MinValue);
		[IgnoreMember]
		public float this[int key]
		{
			get { return (key == 0) ? x : y; }
			set { if (key == 0) x = value; else y = value; }
		}

		[IgnoreMember]
		public float LengthSquared
		{
			get { return x * x + y * y; }
		}
		[IgnoreMember]
		public float Length
		{
			get { return (float)Math.Sqrt(LengthSquared); }
		}

		public float Normalize(float epsilon = MathUtil.Epsilonf)
		{
			float length = Length;
			if (length > epsilon)
			{
				float invLength = 1.0f / length;
				x *= invLength;
				y *= invLength;
			}
			else
			{
				length = 0;
				x = y = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Vector2f Normalized
		{
			get
			{
				float length = Length;
				if (length > MathUtil.Epsilonf)
				{
					float invLength = 1 / length;
					return new Vector2f(x * invLength, y * invLength);
				}
				else
					return Vector2f.Zero;
			}
		}

		[IgnoreMember]
		public bool IsNormalized
		{
			get { return Math.Abs((x * x + y * y) - 1) < MathUtil.ZeroTolerancef; }
		}
		[IgnoreMember]
		public bool IsFinite
		{
			get { float f = x + y; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
		}
		public void Round(int nDecimals)
		{
			x = (float)Math.Round(x, nDecimals);
			y = (float)Math.Round(y, nDecimals);
		}
		public float Dot(Vector2f v2)
		{
			return x * v2.x + y * v2.y;
		}


		public float Cross(Vector2f v2)
		{
			return x * v2.y - y * v2.x;
		}

		[IgnoreMember]
		public Vector2f Perp
		{
			get { return new Vector2f(y, -x); }
		}
		[IgnoreMember]
		public Vector2f UnitPerp
		{
			get { return new Vector2f(y, -x).Normalized; }
		}
		public float DotPerp(Vector2f v2)
		{
			return x * v2.y - y * v2.x;
		}


		public float AngleD(Vector2f v2)
		{
			float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)(Math.Acos(fDot) * MathUtil.Rad2Deg);
		}
		public static float AngleD(Vector2f v1, Vector2f v2)
		{
			return v1.AngleD(v2);
		}
		public float AngleR(Vector2f v2)
		{
			float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)(Math.Acos(fDot));
		}
		public static float AngleR(Vector2f v1, Vector2f v2)
		{
			return v1.AngleR(v2);
		}



		public float DistanceSquared(Vector2f v2)
		{
			float dx = v2.x - x, dy = v2.y - y;
			return dx * dx + dy * dy;
		}
		public float Distance(Vector2f v2)
		{
			float dx = v2.x - x, dy = v2.y - y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}


		public void Set(Vector2f o)
		{
			x = o.x;
			y = o.y;
		}
		public void Set(float fX, float fY)
		{
			x = fX;
			y = fY;
		}
		public void Add(Vector2f o)
		{
			x += o.x;
			y += o.y;
		}
		public void Subtract(Vector2f o)
		{
			x -= o.x;
			y -= o.y;
		}


		public static Vector2f operator -(Vector2f v)
		{
			return new Vector2f(-v.x, -v.y);
		}

		public static Vector2f operator +(Vector2f a, Vector2f o)
		{
			return new Vector2f(a.x + o.x, a.y + o.y);
		}
		public static Vector2f operator +(Vector2f a, float f)
		{
			return new Vector2f(a.x + f, a.y + f);
		}

		public static Vector2f operator -(Vector2f a, Vector2f o)
		{
			return new Vector2f(a.x - o.x, a.y - o.y);
		}
		public static Vector2f operator -(Vector2f a, float f)
		{
			return new Vector2f(a.x - f, a.y - f);
		}

		public static Vector2f operator *(Vector2f a, float f)
		{
			return new Vector2f(a.x * f, a.y * f);
		}
		public static Vector2f operator *(float f, Vector2f a)
		{
			return new Vector2f(a.x * f, a.y * f);
		}
		public static Vector2f operator /(Vector2f v, float f)
		{
			return new Vector2f(v.x / f, v.y / f);
		}
		public static Vector2f operator /(float f, Vector2f v)
		{
			return new Vector2f(f / v.x, f / v.y);
		}

		public static Vector2f operator *(Vector2f a, Vector2f b)
		{
			return new Vector2f(a.x * b.x, a.y * b.y);
		}
		public static Vector2f operator /(Vector2f a, Vector2f b)
		{
			return new Vector2f(a.x / b.x, a.y / b.y);
		}


		public static bool operator ==(Vector2f a, Vector2f b)
		{
			return (a.x == b.x && a.y == b.y);
		}
		public static bool operator !=(Vector2f a, Vector2f b)
		{
			return (a.x != b.x || a.y != b.y);
		}
		public override bool Equals(object obj)
		{
			return this == (Vector2f)obj;
		}
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Vector2f other)
		{
			if (x != other.x)
				return x < other.x ? -1 : 1;
			else if (y != other.y)
				return y < other.y ? -1 : 1;
			return 0;
		}
		public bool Equals(Vector2f other)
		{
			return (x == other.x && y == other.y);
		}


		public bool EpsilonEqual(Vector2f v2, float epsilon)
		{
			return (float)Math.Abs(x - v2.x) <= epsilon &&
				   (float)Math.Abs(y - v2.y) <= epsilon;
		}


		public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
		{
			float s = 1 - t;
			return new Vector2f(s * a.x + t * b.x, s * a.y + t * b.y);
		}
		public static Vector2f Lerp(ref Vector2f a, ref Vector2f b, float t)
		{
			float s = 1 - t;
			return new Vector2f(s * a.x + t * b.x, s * a.y + t * b.y);
		}


		public override string ToString()
		{
			return string.Format("{0:F8} {1:F8}", x, y);
		}

		public unsafe Vector2 ToSystemNumric()
		{
			fixed (Vector2f* vector3f = &this)
			{
				return *(Vector2*)vector3f;
			}
		}

		public static unsafe Vector2f ToRhuNumrics(ref Vector2 value)
		{
			fixed (Vector2* vector3f = &value)
			{
				return *(Vector2f*)vector3f;
			}
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

		public static explicit operator Vector2(Vector2f b) => b.ToSystemNumric();

		public static explicit operator Vector2f(Vector2 b) => ToRhuNumrics(ref b);

	}
}
