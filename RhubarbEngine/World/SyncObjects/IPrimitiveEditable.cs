using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public interface IPrimitiveEditable: IWorldObject, ISyncMember
    {
       public string primitiveString { get; set; }
    }
}
