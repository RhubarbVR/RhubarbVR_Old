using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using Veldrid;
using RhubarbDataTypes;

namespace RhubarbEngine.Render.Material.Fields
{
    public class Texture2DField: MaterialField
    {
        public AssetRef<RTexture2D> field;
        public override void buildSyncObjs(bool newRefIds)
        {
            field = new AssetRef<RTexture2D>(this, newRefIds);
            field.loadChange += assetChange;
        }
        public override void setValue(Object val)
        {
            field.value = (NetPointer)val;
        }
        public void assetChange(RTexture2D newAsset)
        {
            resource = null;
            isNull = false;
        }

        unsafe public override void updateBuffer(GraphicsDevice gb)
        {

        }

        public unsafe override void createDeviceResource(ResourceFactory fact)
        {
            if (resource != null)
            {
                return;
            }
            if (field.Asset.view == null)
            {
                field.Asset.createResource(fact);
                if(field.Asset.view != null)
                {
                    resource = field.Asset.view;
                }
                else
                {
                    isNull = true;
                }
            }
        }
    }
}
