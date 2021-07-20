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
    

    [Category("ImGUI/Begin/Popup")]
    public class ImGUIBeginPopupContextVoid : UIWidgetList
    {


        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);

        }

        public ImGUIBeginPopupContextVoid(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginPopupContextVoid()
        {
        }

        public override void ImguiRender()
        {
            if (ImGui.BeginPopupContextVoid())
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender();
                }
                ImGui.EndPopup();
            }
            }
        }
    }

