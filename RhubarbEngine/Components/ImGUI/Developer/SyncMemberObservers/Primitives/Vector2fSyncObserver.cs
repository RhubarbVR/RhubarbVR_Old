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
    public class Vector2fSyncObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<Sync<Vector2f>> target;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<Sync<Vector2f>>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }


        public Vector2fSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Vector2fSyncObserver()
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
                    if (typeof(Sync<Vector2f>).IsAssignableFrom(type))
                    {
                        Changeboarder = true;
                    }
            }
            if (Changeboarder)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
                ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
            }
            Vector2 val = target.target?.value.ToSystemNumric()??Vector2.Zero;
            if(ImGui.DragFloat2((fieldName.value ?? "null") + $"##{referenceID.id}", ref val,0.1f,-10000, 10000, "%.2f", ImGuiSliderFlags.NoRoundToFormat))
            {
                if(target.target != null)
                    target.target.value = (Vector2f)val;
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
                    Sync<Vector2f> e = (Sync<Vector2f>)source.Referencer.target;
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
