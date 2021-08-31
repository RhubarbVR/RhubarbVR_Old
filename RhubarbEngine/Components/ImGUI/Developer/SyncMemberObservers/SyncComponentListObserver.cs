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
    [Category("ImGUI/Developer/SyncMemberObservers")]
    public class SyncComponentListObserver : SyncListBaseObserver, IObserver
    {
        public override bool removeable => false;

        public SyncComponentListObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncComponentListObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            
            if (ImGui.BeginChild(referenceID.id.ToString(),new Vector2(ImGui.GetWindowContentRegionWidth(), ImGui.GetFrameHeightWithSpacing() - 50f)))
            {
                RenderChildren(imGuiRenderer,canvas);
                ImGui.EndChild();
            }
            if (ImGui.Button($"Attach Component##{referenceID.id}",new Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
            {

            }
        }

    }

}
