using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public interface IWorldObject: IDisposable
    {
        NetPointer ReferenceID { get; }

        World World { get; }

        void addDisposable(IDisposable val);
        IWorldObject Parent { get; }

        bool IsLocalObject { get; }

        bool IsPersistent { get; }

        bool IsRemoved { get; }
        DataNodeGroup serialize(bool netsync = false);

        void deSerialize( DataNodeGroup data , List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>));

    }
}

