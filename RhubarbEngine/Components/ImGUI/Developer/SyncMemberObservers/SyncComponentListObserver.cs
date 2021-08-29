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
    public class SyncComponentListObserver : SyncListBaseObserver, IObserver
    {


        public SyncComponentListObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncComponentListObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            ImGui.Text(fieldName.value ?? "NUll");
            if (ImGui.BeginChild(referenceID.id.ToString()))
            {
                RenderChildren(imGuiRenderer);
                ImGui.EndChild();
            }
        }

    }

}
