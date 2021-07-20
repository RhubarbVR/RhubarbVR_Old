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
    public class ImGUIBeginPopup : UIWidgetList
    {

        public Sync<string> id;

        public Sync<ImGuiWindowFlags> windowflag;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            id = new Sync<string>(this, newRefIds);
            windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
        }

        public ImGUIBeginPopup(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginPopup()
        {
        }

        public override void ImguiRender()
        {
            if(ImGui.BeginPopup(id.value,windowflag.value))
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

