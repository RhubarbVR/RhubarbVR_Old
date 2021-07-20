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

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Begin")]
    public class ImGUIBeginMainMenuBar : UIWidgetList
    {



        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);

        }

        public ImGUIBeginMainMenuBar(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginMainMenuBar()
        {
        }

        public override void ImguiRender()
        {
            if (ImGui.BeginMainMenuBar())
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender();
                }
                ImGui.EndMainMenuBar();
            }
            

            }
        }
    }

