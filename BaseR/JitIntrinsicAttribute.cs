using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseR
{
    /// <summary>
    /// An attribute that can be attached to JIT Intrinsic methods/properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
    internal class JitIntrinsicAttribute : Attribute
    {
    }
}
