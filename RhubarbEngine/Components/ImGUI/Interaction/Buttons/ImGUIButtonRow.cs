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
    
    [Category("ImGUI/Interaction/Button")]
    public class ImGUIButtonRow : UIWidget
    {

        public SyncValueList<string> labels;
        public SyncValueList<float> widths;
        public SyncDelegate<Action<string>> action;
        public Sync<float> hight;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            labels = new SyncValueList<string>(this, newRefIds);
            action = new SyncDelegate<Action<string>>(this, newRefIds);
            widths = new SyncValueList<float>(this, newRefIds);
            hight = new Sync<float>(this, newRefIds);
            hight.value = 0.06666666666666666f;
        }

        public ImGUIButtonRow(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
            
        }
        public ImGUIButtonRow()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                var label = labels[i].value;
                ImGui.SameLine();
                if (label != null)
                {
                    if (ImGui.Button(label, new Vector2(ImGui.GetIO().DisplaySize.X * widths[i]?.value??0, ImGui.GetIO().DisplaySize.Y * hight.value)))
                    {
                        action.Target?.Invoke(label);
                    }
                }
                else
                {
                    ImGui.Dummy(new Vector2(ImGui.GetIO().DisplaySize.X * widths[i]?.value ?? 0, ImGui.GetIO().DisplaySize.Y * hight.value));
                }
            }
            ImGui.Separator();
        }
    }
}
