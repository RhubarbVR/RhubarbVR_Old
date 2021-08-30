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
using g3;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Begin")]
    public class ImGUIBeginChildFrame : UIWidgetList
    {
        public Sync<uint> id;

        public Sync<Vector2f> size;


        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            id = new Sync<uint>(this, newRefIds);
            size = new Sync<Vector2f>(this, newRefIds);

        }

        public ImGUIBeginChildFrame(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginChildFrame()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            if(ImGui.BeginChildFrame(id.value, new Vector2(size.value.x, size.value.y)))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer, canvas);
                }
                ImGui.EndChildFrame();
            }
        }
    }
}
