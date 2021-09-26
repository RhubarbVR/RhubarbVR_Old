using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World
{
	public interface ISyncList : ISyncMember, IEnumerable<IWorldObject>
	{
		public int Count();

        public void Clear();

		public void Remove(int index);

		public bool TryToAddToSyncList();
	}
}
