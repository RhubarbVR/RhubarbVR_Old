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
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Interaction/Text")]
    public class ImGUIInputText : UIWidget
    {

        public Sync<ImGuiInputTextFlags> flags;
        public Sync<string> label;
        public Sync<string> text;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            flags = new Sync<ImGuiInputTextFlags>(this, newRefIds);
            flags.value = ImGuiInputTextFlags.None;
            label = new Sync<string>(this, newRefIds);
            text = new Sync<string>(this, newRefIds);
        }


        public ImGUIInputText(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIInputText()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            string val = text.value?? "";
            ImGui.InputText((label.value ?? "")+$"##{referenceID.id}", ref val, (uint)val.Length + 255, flags.value);
            if (val != text.value)
            {
                text.value = val;
            }
        }
    }
}
