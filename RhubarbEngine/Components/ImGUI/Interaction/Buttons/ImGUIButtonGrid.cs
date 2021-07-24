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
    public class ImGUIButtonGrid : UIWidget
    {

        public SyncValueList<string> labels;
        public Sync<int> Columns;
        public SyncDelegate<Action<string>> action;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            labels = new SyncValueList<string>(this, newRefIds);
            action = new SyncDelegate<Action<string>>(this, newRefIds);
            Columns = new Sync<int>(this, newRefIds);
            Columns.value = 5;
        }

        public ImGUIButtonGrid(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
            
        }
        public ImGUIButtonGrid()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            ImGui.Columns(Columns.value, null);
            ImGui.Separator();
            for (int i = 0; i < labels.Count; i++)
            {
                var label = labels[i].value;
                if(label != null)
                {
                    if (ImGui.Button(label, new Vector2(ImGui.GetIO().DisplaySize.X / Columns.value, ImGui.GetIO().DisplaySize.Y / Columns.value)))
                    {
                        action.Target?.Invoke(label);
                    }
                }
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGui.Separator();
        }
    }
}
