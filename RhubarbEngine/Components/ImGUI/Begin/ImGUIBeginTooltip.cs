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
    public class ImGUIBeginTabBar : UIWidgetList
    {
        public Sync<string> id;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            id = new Sync<string>(this, newRefIds);

        }

        public ImGUIBeginTabBar(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginTabBar()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            if (ImGui.BeginTabBar(id.value ?? ""))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer);
                }
                ImGui.EndTabBar();
            }

        }
        }
    }

