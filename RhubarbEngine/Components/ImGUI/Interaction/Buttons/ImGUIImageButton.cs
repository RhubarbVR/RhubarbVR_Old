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
            size.value = new Vector2f(100,100);
            action = new SyncDelegate(this, newRefIds);
            action.Target = test;
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

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            var draw_list = ImGui.GetWindowDrawList();
            draw_list.AddCircleFilled(ImGui.GetMousePos() + new Vector2(50), 30f, 0xFFFFFFFF);
            if (ImGui.ImageButton(imGuiRenderer.GetOrCreateImGuiBinding(engine.renderManager.gd.ResourceFactory, view), new Vector2(size.value.x, size.value.y)) )
            {
                logger.Log("This is a test of test in test of trains");
                action.Target?.Invoke();
            }
        }
    }
}
