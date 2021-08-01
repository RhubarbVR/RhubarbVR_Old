using RhubarbEngine.World.DataStructure;
using RhubarbEngine.World.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public abstract class UserStream : Worker, IWorldObject, ISyncMember
    {
        public Sync<string> name;

        public override void inturnalSyncObjs(bool newRefIds)
        {
            name = new Sync<string>(this, newRefIds);
        }

        public virtual void ReceiveData(DataNodeGroup data, Peer peer)
        {
        }
        public UserStream()
        {

        }

        public UserStream(World _world, IWorldObject _parent, bool newRefID = true) : base(_world,_parent,newRefID)
        {

        }

    }
}
