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
    public class Texture2DField : MaterialField, IWorldObject
    {
        public AssetRef<RTexture2D> field;
        public override void buildSyncObjs(bool newRefIds)
        {
            field = new AssetRef<RTexture2D>(this, newRefIds);
            field.loadChange += assetChange;
        }

        public override void onUpdate()
        {
            base.onUpdate();
            if (input.mainWindows.GetKey(Key.F3))
            {
                loadTextureView(true);
            }

        }

        public override void setValue(Object val)
        {
            field.value = (NetPointer)val;
        }
        public void assetChange(RTexture2D newAsset)
        {
            loadTextureView();
        }

        private void SetResource(BindableResource res, bool forceR = false)
        {
            if ((resource != res) || forceR)
            {
                resource = res;
                rMaterial.ReloadBindableResources();
            }
        }
        public void loadTextureView(bool forceR = false)
        {
            if(field.target != null)
            {
                if(field.Asset != null)
                {
                    if (field.Asset.view != null)
                    {
                        SetResource(field.Asset.view, forceR);
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
                SetResource(engine.renderManager.solidview);
            }

        }

        public unsafe override void createDeviceResource(ResourceFactory fact)
        {
            loadTextureView();
        }
    }
}
