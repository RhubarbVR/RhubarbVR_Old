using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public class Worker : IWorldObject
    {
        private World world;

        private IWorldObject parent;

        private RefID referenceID;
        RefID IWorldObject.ReferenceID => referenceID;

        World IWorldObject.World => world;

        IWorldObject IWorldObject.Parent => parent;

        bool IWorldObject.IsLocalObject => false;

        bool IWorldObject.IsPersistent => true;

        bool IWorldObject.IsRemoved => false;

        public Worker(World _world, IWorldObject _parent)
        {
            world = _world;
            parent = _parent;
            referenceID = _world.buildRefID();
            _world.addWorldObj(this);
        }
    }
}
