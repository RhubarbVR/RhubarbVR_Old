using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using Veldrid;
using RhubarbDataTypes;
using Veldrid.ImageSharp;
using System.IO;

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
            logger.Log("Texture asset change");
            loadTextureView();
        }

        private void SetResource(BindableResource res)
        {
            if(resource != res)
            {
                resource = res;
                logger.Log("Trains");
                rMaterial.ReloadBindableResources();
            }
        }

        unsafe public override void updateBuffer(GraphicsDevice gb)
        {
            if (resource != null)
            {
                return;
            }
            loadTextureView();
        }

        public void loadTextureView()
        {
            logger.Log("LoadTextureView");
            if(field.target != null)
            {
                if(field.Asset != null)
                {
                    if (field.Asset.view != null)
                    {
                        logger.Log("Trains Loaded Texture");
                        SetResource(field.Asset.view);
                    }
                    else
                    {
                        SetResource(engine.renderManager.nulview);
                    }
                }
                else
                {
                    SetResource(engine.renderManager.nulview);
                }
            }
            else
            {
                SetResource(engine.renderManager.nulview);
            }

        }

        public unsafe override void createDeviceResource(ResourceFactory fact)
        {

            if (resource != null)
            {
                return;
            }
            loadTextureView();
        }
    }
}
