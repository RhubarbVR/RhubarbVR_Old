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
    

    [Category("ImGUI/Begin/Popup")]
    public class ImGUIBeginPopupModal : UIWidgetList
    {
        public Sync<string> name;
        public Sync<bool> open;
        public Sync<ImGuiWindowFlags> windowflag;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            name = new Sync<string>(this, newRefIds);
            windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
            windowflag.value = ImGuiWindowFlags.None;
        }

        public ImGUIBeginPopupModal(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginPopupModal()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            bool lopen = open.value;

            if (ImGui.BeginPopupModal(name.value ?? "", ref lopen, windowflag.value))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer,canvas);
                }
                ImGui.EndPopup();
            }
            if (lopen != open.value)
            {
                open.value = lopen;
            }
        }
        }
    }

