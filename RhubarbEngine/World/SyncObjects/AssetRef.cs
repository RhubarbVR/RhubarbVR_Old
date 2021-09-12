using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.World
{
	public class AssetRef<T> : SyncRef<AssetProvider<T>>, IWorldObject where T : IAsset
	{
		public T Asset
		{
			get
			{
				if (base.Target == null)
				{
					return default(T);
				}
				return base.Target.value;
			}
		}

		public event Action<T> loadChange;

		public void loadedCall(T newAsset)
		{
			loadChange?.Invoke(newAsset);
		}

		public override void Bind()
		{
			base.Bind();
			base.Target.onLoadedCall += loadedCall;
			if (base.Target.loaded)
			{
				loadedCall(Target.value);
			}
		}
		public override void onLoaded()
		{
			base.onLoaded();
			base.Target.onLoadedCall += loadedCall;
			if (base.Target.loaded)
			{
				loadedCall(base.Target.value);
			}
		}
		public AssetRef(IWorldObject _parent, bool newrefid = true) : base(_parent, newrefid)
		{
		}
	}
}
