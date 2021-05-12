using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseR
{
    internal static class HashCodeHelper
    {
        /// <summary>
        /// Combines two hash codes, useful for combining hash codes of individual vector elements
        /// </summary>
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
    }
}
