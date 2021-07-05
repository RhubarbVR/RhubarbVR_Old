using MessagePack;
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
    [MessagePackObject]
    public struct Vector4i : IComparable<Vector4i>, IEquatable<Vector4i>
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;
        [Key(2)]
        public int z;
        [Key(3)]
        public int w;

        public Vector4i(int f) { x = y = z = w = f; }
        public Vector4i(int x, int y, int z,int w) { this.x = x; this.y = y; this.z = z;this.w = w;  }
        public Vector4i(int[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }

        static public readonly Vector4i Zero = new Vector4i(0, 0, 0,0);
        static public readonly Vector4i One = new Vector4i(1, 1, 1,1);
        static public readonly Vector4i AxisX = new Vector4i(1, 0, 0,0);
        static public readonly Vector4i AxisY = new Vector4i(0, 1, 0, 0);
        static public readonly Vector4i AxisZ = new Vector4i(0, 0, 1, 0);
        static public readonly Vector4i AxisW = new Vector4i(0, 0, 0,1);

        public int this[int key]
        {
            get { return (key == 0) ? x : (key == 1) ? y : (key == 2) ? w : z; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else if (key == 3) w = value; else z = value; ; }
        }

        public int[] array {
            get { return new int[] { x, y, z,w }; }
        }



        public void Set(Vector4i o)
        {
            x = o.x; y = o.y; z = o.z; w = o.w;
        }
        public void Set(int fX, int fY, int fZ, int fW)
        {
            x = fX; y = fY; z = fZ; w = fW;
        }
        public void Add(Vector4i o)
        {
            x += o.x; y += o.y; z += o.z; w += o.w;
        }
        public void Subtract(Vector4i o)
        {
            x -= o.x; y -= o.y; z -= o.z; w -= o.w;
        }
        public void Add(int s) { x += s;  y += s;  z += s;  w += s; }


        public int LengthSquared { get { return x * x + y * y + z * z;  } }


        public static Vector4i operator -(Vector4i v)
        {
            return new Vector4i(-v.x, -v.y, -v.z, -v.w);
        }

        public static Vector4i operator *(int f, Vector4i v)
        {
            return new Vector4i(f * v.x, f * v.y, f * v.z , f * v.w);
        }
        public static Vector4i operator *(Vector4i v, int f)
        {
            return new Vector4i(f * v.x, f * v.y, f * v.z, f * v.w);
        }
        public static Vector4i operator /(Vector4i v, int f)
        {
            return new Vector4i(v.x /f, v.y /f, v.z /f, v.w / f);
        }
        public static Vector4i operator /(int f, Vector4i v)
        {
            return new Vector4i(f / v.x, f / v.y, f / v.z, v.w / f);
        }

        public static Vector4i operator *(Vector4i a, Vector4i b)
        {
            return new Vector4i(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }
        public static Vector4i operator /(Vector4i a, Vector4i b)
        {
            return new Vector4i(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }


        public static Vector4i operator +(Vector4i v0, Vector4i v1)
        {
            return new Vector4i(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
        }
        public static Vector4i operator +(Vector4i v0, int f)
        {
            return new Vector4i(v0.x + f, v0.y + f, v0.z + f, v0.w + f);
        }

        public static Vector4i operator -(Vector4i v0, Vector4i v1)
        {
            return new Vector4i(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
        }
        public static Vector4i operator -(Vector4i v0, int f)
        {
            return new Vector4i(v0.x - f, v0.y - f, v0.z - f, v0.w - f);
        }




        public static bool operator ==(Vector4i a, Vector4i b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
        }
        public static bool operator !=(Vector4i a, Vector4i b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector4i)obj;
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
        public int CompareTo(Vector4i other)
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
        public bool Equals(Vector4i other)
        {
            return (x == other.x && y == other.y && z == other.z && w == other.w);
        }



        public override string ToString() {
            return string.Format("{0} {1} {2}", x, y, z);
        }



        // implicit cast between Index4i and Vector4i
        public static implicit operator Vector4i(Index4i v) {
            return new Vector4i(v.a, v.b, v.c,v.d);
        }
        public static implicit operator Index4i(Vector4i v) {
            return new Index4i(v.x, v.y, v.z,v.w);
        }


    }
}
