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
    

    [Category("ImGUI/Interaction")]
    public class ImGUIEnumSelector<T> : UIWidget where T: System.Enum
    {

        public Sync<string> label;

        public Sync<T> value;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            label = new Sync<string>(this, newRefIds);
            value = new Sync<T>(this, newRefIds);
        }

        public ImGUIEnumSelector(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIEnumSelector()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            int c = (int)(object)value.value;
            var e = Enum.GetNames(typeof(T)).ToList();
            ImGui.Combo(label.value ?? "", ref c, e.ToArray(), e.Count);
            if(c != (int)(object)value.value)
            {
                value.value = (T)(object)c;
            }
        }
    }
}
