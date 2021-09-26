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
                return base.Target == null ? default : base.Target.Value;
            }
        }

		public event Action<T> LoadChange;

		public void LoadedCall(T newAsset)
		{
			LoadChange?.Invoke(newAsset);
		}

		public override void Bind()
		{
			base.Bind();
			base.Target.OnLoadedCall += LoadedCall;
			if (base.Target.loaded)
			{
				LoadedCall(Target.Value);
			}
		}
		public override void OnLoaded()
		{
			base.OnLoaded();
			base.Target.OnLoadedCall += LoadedCall;
			if (base.Target.loaded)
			{
				LoadedCall(base.Target.Value);
			}
		}
        public AssetRef()
        {

        }

        public AssetRef(IWorldObject _parent, bool newrefid = true) : base(_parent, newrefid)
		{
		}
	}
}
