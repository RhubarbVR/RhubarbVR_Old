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
using RhubarbEngine.World.Asset;
using g3;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{


    [Category("ImGUI/Interaction/Button")]
    public class ImGUIImageButton : UIWidget
    {
        
        public AssetRef<RTexture2D> texture;
        public Sync<Vector2f> size;
        public Sync<Vector2f> UV0;
        public Sync<Vector2f> UV1;
        public Sync<Colorf> tint;
        public Sync<Colorf> big;
        public Sync<int> padding;
        public Sync<bool> TintOnClick;
        public Sync<float> TintOnClickTime;

        public SyncDelegate action;
        private TextureView view;

        public override void onLoaded()
        {
            base.onLoaded();
            loadTextureView();
        }

        private void test()
        {
            logger.Log("This is a test of test in test");
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);

            texture = new AssetRef<RTexture2D>(this, newRefIds);
            texture.loadChange += assetChange;
            size = new Sync<Vector2f>(this, newRefIds);
            size.value = new Vector2f(100, 100);
            UV0 = new Sync<Vector2f>(this, newRefIds);
            UV0.value = new Vector2f(0, 0);
            UV1 = new Sync<Vector2f>(this, newRefIds);
            UV1.value = new Vector2f(1, 1);
            padding = new Sync<int>(this, newRefIds);
            padding.value = 0;
            tint = new Sync<Colorf>(this, newRefIds);
            tint.value = Colorf.White;
            big = new Sync<Colorf>(this, newRefIds);
            big.value = Colorf.White;
            action = new SyncDelegate(this, newRefIds);
            action.Target = test;
            TintOnClick = new Sync<bool>(this, newRefIds);
            TintOnClick.value = true;
            TintOnClickTime = new Sync<float>(this, newRefIds);
            TintOnClickTime.value = 0.1f;
        }
        public void assetChange(RTexture2D newAsset)
        {
            loadTextureView();
        }

        public void loadTextureView()
        {
            if (texture.target != null)
            {
                if (texture.Asset != null)
                {
                    if (texture.Asset.view != null)
                    {
                        SetResource(texture.Asset.view);
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
        private void SetResource(TextureView res)
        {
            if (view != res)
            {
                view = res;
            }
        }

        public ImGUIImageButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public ImGUIImageButton()
        {
        }

        private DateTime lastClick;

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            var tintval = tint.value;
            if (TintOnClick.value & (DateTime.UtcNow - lastClick) < new TimeSpan(0, 0, 0,0,(int)(TintOnClickTime.value*1000)))
            {
                tintval *= 0.8f;
            }
            if (ImGui.ImageButton(imGuiRenderer.GetOrCreateImGuiBinding(engine.renderManager.gd.ResourceFactory, view), new Vector2(size.value.x, size.value.y), new Vector2(UV0.value.x, UV0.value.y), new Vector2(UV1.value.x, UV1.value.y),padding.value, big.value.ToRGBA().ToSystem(), tintval.ToRGBA().ToSystem()) )
            {
                if (TintOnClick.value)
                {
                    lastClick = DateTime.UtcNow;
                }
                action.Target?.Invoke();
            }
        }
    }
}
