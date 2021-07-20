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
    public class ImGUIBeginMenu : UIWidgetList
    {

        public Sync<string> label;

        public Sync<bool> Uienabled;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            label = new Sync<string>(this, newRefIds);
            Uienabled = new Sync<bool>(this, newRefIds);
        }

        public ImGUIBeginMenu(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginMenu()
        {
        }

        public override void ImguiRender()
        {
            if(ImGui.BeginMenu(label.noneNullValue, Uienabled.value))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender();
                }
                ImGui.EndMenu();
            }
            

            }
        }
    }

