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
    [Category("ImGUI/Developer/SyncMemberObservers")]
    public class SyncRefObserver<T> : SyncRefObserver, IObserver
    {

        public SyncRefObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncRefObserver()
        {
        }

        public unsafe override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            bool Changeboarder = false;
            if (target.target?.Driven ?? false)
            {
                var e = ImGui.GetStyleColorVec4(ImGuiCol.FrameBg);
                var vec = (Vector4f)(*e);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, (vec - new Vector4f(0, 1f, 0, 0)).ToSystem());
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
                //check if is type
                var type = source.Referencer.target?.GetType();
                if (typeof(T).IsAssignableFrom(type))
                {
                    Changeboarder = true;
                }
            }
            if (Changeboarder)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
                ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
            }
            string val =  "null";
            if(target.target != null)
            {
                val = $"{target.target?.targetIWorldObject.getNameString()??""} ID:({target.target?.targetIWorldObject?.ReferenceID.id.ToHexString()??"null"})";
            }
            if (ImGui.Button("^##" + referenceID.id.ToString()))
            {
                target.target.targetIWorldObject?.openWindow();
            }
            ImGui.SameLine();
            if (ImGui.Button("X##" + referenceID.id.ToString()))
            {
                if(target.target != null)
                    target.target.value = default(NetPointer);
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.CalcItemWidth()-45);
            ImGui.InputText((fieldName.value ?? "null") + $"##{referenceID.id}", ref val, (uint)val.Length + 255, ImGuiInputTextFlags.ReadOnly);
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                if (source != null)
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
                    //OnDrop
                }
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
            }
        }
    }



    public class SyncRefObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<ISyncRef> target;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<ISyncRef>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }


        public SyncRefObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncRefObserver()
        {
        }
    }
}
