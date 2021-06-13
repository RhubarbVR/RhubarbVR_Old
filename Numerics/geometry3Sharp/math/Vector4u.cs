using System;
using System.Collections.Generic;
using System.Text;

namespace g3
{
    /// <summary>
    /// 3D integer vector type. This is basically the same as Index3i but
    /// with .x.y.z member names. This makes code far more readable in many places.
    /// Unfortunately I can't see a way to do this w/o so much duplication...we could
    /// have .x/.y/.z accessors but that is much less efficient...
    /// </summary>
    public struct Vector4u : IComparable<Vector4u>, IEquatable<Vector4u>
    {
        public uint x;
        public uint y;
        public uint z;
        public uint w;

        public Vector4u(uint f) { x = y = z = w = f; }
        public Vector4u(uint x, uint y, uint z,uint w) { this.x = x; this.y = y; this.z = z;this.w = w;  }
        public Vector4u(uint[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }

        static public readonly Vector4u Zero = new Vector4u(0, 0, 0,0);
        static public readonly Vector4u One = new Vector4u(1, 1, 1,1);
        static public readonly Vector4u AxisX = new Vector4u(1, 0, 0,0);
        static public readonly Vector4u AxisY = new Vector4u(0, 1, 0, 0);
        static public readonly Vector4u AxisZ = new Vector4u(0, 0, 1, 0);
        static public readonly Vector4u AxisW = new Vector4u(0, 0, 0,1);

        public uint this[uint key]
        {
            get { return (key == 0) ? x : (key == 1) ? y : (key == 2) ? w : z; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else if (key == 3) w = value; else z = value; ; }
        }

        public uint[] array {
            get { return new uint[] { x, y, z,w }; }
        }



        public void Set(Vector4u o)
        {
            x = o.x; y = o.y; z = o.z; w = o.w;
        }
        public void Set(uint fX, uint fY, uint fZ, uint fW)
        {
            x = fX; y = fY; z = fZ; w = fW;
        }
        public void Add(Vector4u o)
        {
            x += o.x; y += o.y; z += o.z; w += o.w;
        }
        public void Subtract(Vector4u o)
        {
            x -= o.x; y -= o.y; z -= o.z; w -= o.w;
        }
        public void Add(uint s) { x += s;  y += s;  z += s;  w += s; }


        public uint LengthSquared { get { return x * x + y * y + z * z;  } }


        public static Vector4u operator -(Vector4u v)
        {
            return new Vector4u((uint)-(int)v.x, (uint)-(int)v.y, (uint)-(int)v.z, (uint)-(int)v.w);
        }

        public static Vector4u operator *(uint f, Vector4u v)
        {
            return new Vector4u(f * v.x, f * v.y, f * v.z , f * v.w);
        }
        public static Vector4u operator *(Vector4u v, uint f)
        {
            return new Vector4u(f * v.x, f * v.y, f * v.z, f * v.w);
        }
        public static Vector4u operator /(Vector4u v, uint f)
        {
            return new Vector4u(v.x /f, v.y /f, v.z /f, v.w / f);
        }
        public static Vector4u operator /(uint f, Vector4u v)
        {
            return new Vector4u(f / v.x, f / v.y, f / v.z, v.w / f);
        }

        public static Vector4u operator *(Vector4u a, Vector4u b)
        {
            return new Vector4u(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }
        public static Vector4u operator /(Vector4u a, Vector4u b)
        {
            return new Vector4u(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }


        public static Vector4u operator +(Vector4u v0, Vector4u v1)
        {
            return new Vector4u(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
        }
        public static Vector4u operator +(Vector4u v0, uint f)
        {
            return new Vector4u(v0.x + f, v0.y + f, v0.z + f, v0.w + f);
        }

        public static Vector4u operator -(Vector4u v0, Vector4u v1)
        {
            return new Vector4u(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
        }
        public static Vector4u operator -(Vector4u v0, uint f)
        {
            return new Vector4u(v0.x - f, v0.y - f, v0.z - f, v0.w - f);
        }




        public static bool operator ==(Vector4u a, Vector4u b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
        }
        public static bool operator !=(Vector4u a, Vector4u b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector4u)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int) 2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                hash = (hash * 16777619) ^ w.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector4u other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            else if (z != other.z)
                return z < other.z ? -1 : 1;
            else if (w != other.w)
                return w < other.w ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector4u other)
        {
            return (x == other.x && y == other.y && z == other.z && w == other.w);
        }



        public override string ToString() {
            return string.Format("{0} {1} {2}", x, y, z);
        }



    }
}
