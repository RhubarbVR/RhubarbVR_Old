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

    [Category("ImGUI/Developer/SyncMemberObservers/Primitives")]
    public class Vector4dSyncObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<Sync<Vector4d>> target;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<Sync<Vector4d>>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }


        public Vector4dSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Vector4dSyncObserver()
        {
        }

        public unsafe override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            bool Changeboarder = false;
            if (target.target?.Driven ?? false)
            {
                var e = ImGui.GetStyleColorVec4(ImGuiCol.FrameBg);
                var vec = (Vector4f)(*e);
                ImGui.PushStyleColor(ImGuiCol.FrameBg,(vec - new Vector4f(0,1f,0,0)).ToSystem());
            }
            Interaction.GrabbableHolder source = null;
            switch (canvas.imputPlane.target?.source ?? Interaction.InteractionSource.None)
            {
                case Interaction.InteractionSource.LeftLaser:
                    source = world.LeftLaserGrabbableHolder;
                    break;
                case Interaction.InteractionSource.RightLaser:
                    source = world.RightLaserGrabbableHolder;
                    break;
                case Interaction.InteractionSource.HeadLaser:
                    source = world.HeadLaserGrabbableHolder;
                    break;
                default:
                    break;
            }
            if (source != null)
            {
                    var type = source.Referencer.target?.GetType();
                    if (typeof(Sync<Vector4d>).IsAssignableFrom(type))
                    {
                        Changeboarder = true;
                    }
            }
            if (Changeboarder)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
                ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
            }
            Vector4d val = target.target?.value ?? Vector4d.Zero;
                if (ImGui.DragScalarN((fieldName.value ?? "null") + $"##{referenceID.id}", ImGuiDataType.Double, (IntPtr)(&val), 3, 0.1f))
                {
                    if (target.target != null)
                        target.target.value = val;
                }
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                if(source != null)
                {
                    source.Referencer.target = target.target;
                }
            }
            if (target.target?.Driven ?? false)
            {
                ImGui.PopStyleColor();
            }
            if (Changeboarder)
            {
                if (ImGui.IsItemHovered() && source.DropedRef)
                {
                    Sync<Vector4d> e = (Sync<Vector4d>)source.Referencer.target;
                    if (target.target != null)
                        target.target.value = e.value;
                    source.Referencer.target = null;
                }
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
            }
        }
    }
}
