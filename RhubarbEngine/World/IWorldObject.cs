using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public interface IWorldObject: IDisposable
    {
        NetPointer ReferenceID { get; }

        World World { get; }

        IWorldObject Parent { get; }

        bool IsLocalObject { get; }

        bool IsPersistent { get; }

        bool IsRemoved { get; }
        DataNodeGroup serialize();

        void deSerialize(DataNodeGroup data,bool NewRefIDs = false, Dictionary<NetPointer, NetPointer> newRefID = default(Dictionary<NetPointer, NetPointer>), Dictionary<NetPointer, RefIDResign> latterResign = default(Dictionary<NetPointer, RefIDResign>));

    }
}

