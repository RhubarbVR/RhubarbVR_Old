using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;
using System.Collections;

namespace RhubarbEngine.World
{
	public class SyncValueList<T> : SyncObjList<Sync<T>>, IWorldObject where T : IConvertible
	{
        public SyncValueList() { }

		public SyncValueList(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public SyncValueList(IWorldObject _parent, bool newRefID) : base( _parent, newRefID)
		{

		}
	}
}
