using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.ECS
{
    public abstract class Component: Worker
    {
        Sync<int> updateOrder;
        public override void inturnalSyncObjs(bool newRefIds)
        {
            updateOrder = new Sync<int>(this, newRefIds);
        }

        public virtual void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }
        public Component(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
        {

        }
        public Component()
        {
        }
    }
}
