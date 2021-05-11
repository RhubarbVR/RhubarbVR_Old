using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
    public class Sync<T> : Worker
    {

        private T _value;

        public T value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public Sync(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }

        public Sync(IWorldObject _parent) : base(_parent.World, _parent)
        {

        }
    }
}
