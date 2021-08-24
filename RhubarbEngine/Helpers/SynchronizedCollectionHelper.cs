using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine
{
    public static class SynchronizedCollectionHelper
    {
        public static bool SafeAdd<T>(this SynchronizedCollection<T> col, T value)
        {
            if (!col.Contains(value))
            {
                col.Add(value);
                return true;
            }
            return false;
        }
    }
}
