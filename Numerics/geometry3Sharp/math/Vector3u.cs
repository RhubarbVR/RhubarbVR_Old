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
    public struct Vector3u : IComparable<Vector3u>, IEquatable<Vector3u>
    {
        [Key(0)]
        public uint x;
        [Key(1)]
        public uint y;
        [Key(2)]
        public uint z;

        public Vector3u(uint f) { x = y = z = f; }
        public Vector3u(uint x, uint y, uint z) { this.x = x; this.y = y; this.z = z; }
        public Vector3u(uint[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }

        static public readonly Vector3u Zero = new Vector3u(0, 0, 0);
        static public readonly Vector3u One = new Vector3u(1, 1, 1);
        static public readonly Vector3u AxisX = new Vector3u(1, 0, 0);
        static public readonly Vector3u AxisY = new Vector3u(0, 1, 0);
        static public readonly Vector3u AxisZ = new Vector3u(0, 0, 1);

        public uint this[uint key]
        {
            get { return (key == 0) ? x : (key == 1) ? y : z; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else z = value; }
        }

        public uint[] array {
            get { return new uint[] { x, y, z }; }
        }



        public void Set(Vector3u o)
        {
            x = o.x; y = o.y; z = o.z;
        }
        public void Set(uint fX, uint fY, uint fZ)
        {
            x = fX; y = fY; z = fZ;
        }
        public void Add(Vector3u o)
        {
            x += o.x; y += o.y; z += o.z;
        }
        public void Subtract(Vector3u o)
        {
            x -= o.x; y -= o.y; z -= o.z;
        }
        public void Add(uint s) { x += s;  y += s;  z += s; }


        public uint LengthSquared { get { return x * x + y * y + z * z; } }


        public static Vector3u operator -(Vector3u v)
        {
            return new Vector3u((uint)-(int)v.x, (uint)-(int)v.y, (uint)-(int)v.z);
        }

        public static Vector3u operator *(uint f, Vector3u v)
        {
            return new Vector3u(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3u operator *(Vector3u v, uint f)
        {
            return new Vector3u(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3u operator /(Vector3u v, uint f)
        {
            return new Vector3u(v.x /f, v.y /f, v.z /f);
        }
        public static Vector3u operator /(uint f, Vector3u v)
        {
            return new Vector3u(f / v.x, f / v.y, f / v.z);
        }

        public static Vector3u operator *(Vector3u a, Vector3u b)
        {
            return new Vector3u(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3u operator /(Vector3u a, Vector3u b)
        {
            return new Vector3u(a.x / b.x, a.y / b.y, a.z / b.z);
        }


        public static Vector3u operator +(Vector3u v0, Vector3u v1)
        {
            return new Vector3u(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
        }
        public static Vector3u operator +(Vector3u v0, uint f)
        {
            return new Vector3u(v0.x + f, v0.y + f, v0.z + f);
        }

        public static Vector3u operator -(Vector3u v0, Vector3u v1)
        {
            return new Vector3u(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
        }
        public static Vector3u operator -(Vector3u v0, uint f)
        {
            return new Vector3u(v0.x - f, v0.y - f, v0.z - f);
        }




        public static bool operator ==(Vector3u a, Vector3u b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Vector3u a, Vector3u b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3u)obj;
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
                return hash;
            }
        }
        public int CompareTo(Vector3u other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            else if (z != other.z)
                return z < other.z ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector3u other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }



        public override string ToString() {
            return string.Format("{0} {1} {2}", x, y, z);
        }



        // implicit cast between Index3i and Vector3i
        public static implicit operator Vector3u(Index3i v) {
            return new Vector3i(v.a, v.b, v.c);
        }
        public static implicit operator Vector3u(Vector3i v) {
            return new Index3i(v.x, v.y, v.z);
        }


    }
}
