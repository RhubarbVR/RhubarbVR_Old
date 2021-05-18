using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.ECS
{
    public abstract class Component : Worker
    {
        public Sync<int> updateOrder;

        private SyncRef<Entity> _entity;

        public Entity entity{ get { return _entity.target; } }
        public override void inturnalSyncObjs(bool newRefIds)
        {
            updateOrder = new Sync<int>(this, newRefIds);
            _entity = new SyncRef<Entity>(this, newRefIds);
        }

        public virtual void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }
        public Component(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
        {
        }

        public override void initialize(World _world, IWorldObject _parent, bool newRefID = true)
        {
            base.initialize(_world, _parent, newRefID);
            _entity.target = (Entity)(_parent.Parent);
        }

        public Component()
        {
        }
    }
}
