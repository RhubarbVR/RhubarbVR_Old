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
        public Sync<Vector2> size;

        public SyncDelegate action;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            
            texture = new AssetRef<RTexture2D>(this, newRefIds);
            size = new Sync<Vector2>(this, newRefIds);

            action = new SyncDelegate(this, newRefIds);
        }

        public ImGUIImageButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public ImGUIImageButton()
        {
        }

        public override void ImguiRender()
        {
            //if (ImGui.ImageButton(/*parent ImGUICanvas?.igr.GetOrCreateImGuiBinding(ResourceFactory factory, texture.Asset.view)*/, size.value) )
            {
                action.Target?.Invoke();
            }
        }
    }
}
