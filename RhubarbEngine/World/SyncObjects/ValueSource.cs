using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public interface ValueSource<T> : IChangeable,IWorldObject
    {
        T value { get; set; }

    }
}
