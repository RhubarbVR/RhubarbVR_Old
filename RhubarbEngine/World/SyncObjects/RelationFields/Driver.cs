using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public class Driver<T> : SyncRef<IDriveMember<T>>, IDriver where T : IConvertible
	{
        public Driver() { }

        public T Drivevalue
		{
			get
			{
				return Target.Value;
			}
			set
			{
				Target.Value = value;
			}
		}

		private IDriveable _driven;

		public bool Linked { get { return _driven != null; } }

		public void SetDriveLocation(IDriveable val)
		{
			if (Target == val)
			{
				_driven = val;
			}
			else
			{
				if (Linked)
				{
					UnLink();
				}
				_driven = val;
			}
		}
		public void RemoveDriveLocation()
		{
			_driven = null;
		}
		public void SetDriveTarget(IDriveMember<T> Target)
		{
			base.Target = Target;
		}
		public override void OnChanged()
		{
			if (Target != null)
			{
				Link();
			}
		}
		private void Link()
		{
			if (Linked)
			{
				UnLink();
			}
			Target.Drive(this);
		}
		private void UnLink()
		{
			if (_driven != null)
			{
				_driven.KillDrive();
			}
		}

        public override void Dispose()
        {
            UnLink();
            base.Dispose();
        }

        public Driver(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public Driver(IWorldObject _parent) : base(_parent.World, _parent)
		{

		}

		public Driver(IWorldObject _parent, bool newrefid = true) : base(_parent, newrefid)
		{

		}
	}
}
