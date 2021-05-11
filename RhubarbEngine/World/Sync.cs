using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;

namespace RhubarbEngine.World
{
    public class Sync<T> : Worker<Sync<T>>
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

        public DataNodeGroup serialize()
        {
            DataNodeGroup obj = new DataNodeGroup();
            DataNode<RefID> Refid = new DataNode<RefID>(referenceID);
            obj.setValue("referenceID", Refid);
            DataNode<T> Value = new DataNode<T>(_value);
            obj.setValue("Value", Value);
            return obj;
        }

        public void deSerialize(DataNodeGroup data)
        {

        }
    }
}
