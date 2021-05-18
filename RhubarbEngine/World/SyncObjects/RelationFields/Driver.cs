using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public class Driver<T> : SyncRef<DriveMember<T>>, IDriver
    {
        public event Action<IChangeable> Changed;
        public T Drivevalue
        {
            get
            {
                return target.value;
            }
            set
            {
                target.value = value;
            }
        }

        private Driveable driven;

        public bool Linked { get { return (driven != null); } }

        public void SetDriveLocation(Driveable val)
        {
            if (target == val)
            {
                driven = val;
            }
            else {
                if (Linked)
                {
                    unLink();
                }
                driven = val;
            }
        }
        public void RemoveDriveLocation()
        {
            driven = null;
        }
        public void setDriveTarget(DriveMember<T> Target)
        {
            target = Target;
        }
        public override void onChanged()
        {
            if(target != null)
            {
                link();
            }
        }
        private void link()
        {
            if (Linked)
            {
                unLink();
            }
            target.drive(this);
        }
        private void unLink()
        {
            if(driven != null)
            {
                driven.killDrive();
            }
        }
        public Driver(World _world, IWorldObject _parent) : base(_world, _parent)
        {

        }
        public Driver(IWorldObject _parent) : base(_parent.World, _parent)
        {

        }

        public Driver(IWorldObject _parent,bool newrefid = true) : base(_parent, newrefid)
        {

        }
    }
}
