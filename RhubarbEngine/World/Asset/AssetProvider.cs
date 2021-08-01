using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;

namespace RhubarbEngine.World.Asset
{
    public abstract class AssetProvider<A> : Component where A: IAsset
    {
        private A _value;

        public event Action<A> onLoadedCall;

        public A value{get{
                return _value;
        } }

        public void load(A data,bool desposeold = false)
        {
            var temp = _value;
            _value = data;
            loaded = (data != null);
            onLoadedCall?.Invoke(data);
            if(temp != null && desposeold)
            {
                temp.Dispose();
            }
        }

        public bool loaded = false;
        
        public AssetProvider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public AssetProvider()
        {
        }
    }
}
