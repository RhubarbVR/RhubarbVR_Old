using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BaseR
{
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
