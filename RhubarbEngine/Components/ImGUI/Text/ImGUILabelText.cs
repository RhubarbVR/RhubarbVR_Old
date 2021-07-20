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
    

    [Category("ImGUI/Text")]
    public class ImGUILabelText : UIWidget
    {

        public Sync<string> text;
        public Sync<string> label;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            text = new Sync<string>(this, newRefIds);
            label = new Sync<string>(this, newRefIds);
        }

        public ImGUILabelText(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUILabelText()
        {
        }

        public override void ImguiRender()
        {
            ImGui.LabelText(label.noneNullValue, text.noneNullValue);
        }
    }
}
