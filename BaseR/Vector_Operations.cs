using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace BaseR
{
    public partial struct Vector2
    {
        public System.Numerics.Vector2 ToSystemNumrics()
        {
            return new System.Numerics.Vector2(this.X, this.Y);
        }
    }

    public partial struct Vector3
    {
        public System.Numerics.Vector3 ToSystemNumrics()
        {
            return new System.Numerics.Vector3(this.X, this.Y,this.Z);
        }
    }

    public partial struct Vector4
    {
        public System.Numerics.Vector4 ToSystemNumrics()
        {
            return new System.Numerics.Vector4(this.X, this.Y, this.Z,this.W);
        }
    }


    /// <summary>
    /// Contains various methods useful for creating, manipulating, combining, and converting generic vectors with one another.
    /// </summary>
    internal static class Vector
        {
        // Every operation must either be a JIT intrinsic or implemented over a JIT intrinsic
        // as a thin wrapper
        // Operations implemented over a JIT intrinsic should be inlined
        // Methods that do not have a <T> type parameter are recognized as intrinsics
        /// <summary>
        /// Returns whether or not vector operations are subject to hardware acceleration through JIT intrinsic support.
        /// </summary>
        [JitIntrinsic]
        public static bool IsHardwareAccelerated
        {
            get
            {
                return false;
            }
        }
    }
}
