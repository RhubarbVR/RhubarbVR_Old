using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public interface ValueSource<T> : IChangeable,IWorldObject where T: IConvertible
    {
        T value { get; set; }

    }
}
