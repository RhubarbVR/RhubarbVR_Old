using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using System.Collections;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
	public class SyncAssetRefList<T> : SyncObjList<AssetRef<T>>where T :class, IAsset
	{
        public SyncAssetRefList() { }
        
		public new AssetRef<T> this[int i]
		{
			get
			{
				return base[i];
			}
		}

		public int Length { get { return base.Count(); } }

		public new IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < base.Count(); i++)
			{
				yield return this[i].Asset;
			}
		}

		public int IndexOf(AssetProvider<T> val)
		{
			var returnint = -1;
			for (var i = 0; i < base.Count(); i++)
			{
				if (this[i].Target == val)
				{
					returnint = i;
					return returnint;
				}
			}
			return returnint;
		}

        public override void OnElementBind(AssetRef<T> element)
        {
            base.OnElementBind(element);
            element.LoadChange += OnLoad;
        }

        private void OnLoad(T val)
		{
			loadChange?.Invoke(val);
		}

		public Action<T> loadChange;

		public SyncAssetRefList(IWorldObject _parent, bool newref = true) : base(_parent, newref)
		{

		}

    }
}
