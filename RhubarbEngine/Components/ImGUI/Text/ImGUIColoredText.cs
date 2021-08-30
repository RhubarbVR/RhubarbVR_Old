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
    

    [Category("ImGUI/Text")]
    public class ImGUIColoredText : UIWidget
    {

        public Sync<string> text;
        public Sync<Colorf> color;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            text = new Sync<string>(this, newRefIds);
            color = new Sync<Colorf>(this, newRefIds);
            color.value = Colorf.Cyan;
        }

        public ImGUIColoredText(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIColoredText()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            ImGui.TextColored(color.value.ToRGBA().ToSystem(), text.value ?? "");
        }
    }
}
