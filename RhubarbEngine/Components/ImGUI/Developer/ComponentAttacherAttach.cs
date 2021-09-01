using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using g3;
using ImGuiNET;
using System.Numerics;
using System.Linq;

namespace RhubarbEngine.Components.ImGUI
{

    [Category("ImGUI/Developer")]
    public class ComponentAttacherAttach : ComponentAttacherField
    {
        public Sync<string> type;

        private Type setType;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            type = new Sync<string>(this, newRefIds);
            type.Changed += Type_Changed;
        }

        private void Type_Changed(IChangeable obj)
        {
            setType = Type.GetType(type.value);
        }


        public ComponentAttacherAttach(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ComponentAttacherAttach()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, (Vector4)Colorf.DarkBlue.ToRGBA());
            if (ImGui.Button(setType.GetFormattedName()??"Null" + "##" + referenceID.id, new Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
            {
                if ((target.target != null)&&(setType != null))
                    target.target.AttachComponent(setType);
            }
            ImGui.PopStyleColor();
        }
    }
}