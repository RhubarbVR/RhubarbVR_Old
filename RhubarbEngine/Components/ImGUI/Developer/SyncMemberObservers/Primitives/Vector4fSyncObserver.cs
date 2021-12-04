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
using RNumerics;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{

	[Category("ImGUI/Developer/SyncMemberObservers/Primitives")]
	public class Vector4fSyncObserver : UIWidget, IPropertiesElement
	{
		public Sync<string> fieldName;

		public SyncRef<Sync<Vector4f>> target;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<Sync<Vector4f>>(this, newRefIds);
			fieldName = new Sync<string>(this, newRefIds);
		}


		public Vector4fSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public Vector4fSyncObserver()
		{
		}

        public override void Destroy()
        {
            base.Destroy();
            Entity.Destroy();
        }
        public unsafe override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var Changeboarder = false;
			if (target.Target?.Driven ?? false)
			{
				var e = ImGui.GetStyleColorVec4(ImGuiCol.FrameBg);
				var vec = (Vector4f)(*e);
				ImGui.PushStyleColor(ImGuiCol.FrameBg, (vec - new Vector4f(0, 1f, 0, 0)).ToSystem());
			}
			Interaction.GrabbableHolder source = null;
			switch (canvas.imputPlane.Target?.Source ?? Interaction.InteractionSource.None)
			{
				case Interaction.InteractionSource.LeftLaser:
					source = World.LeftLaserGrabbableHolder;
					break;
				case Interaction.InteractionSource.RightLaser:
					source = World.RightLaserGrabbableHolder;
					break;
				case Interaction.InteractionSource.HeadLaser:
					source = World.HeadLaserGrabbableHolder;
					break;
				default:
					break;
			}
			if (source != null)
			{
				var type = source.HolderReferen?.GetType();
				if (typeof(Sync<Vector4f>).IsAssignableFrom(type))
				{
					Changeboarder = true;
				}
			}
			if (Changeboarder)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
				ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
			}
			var val = target.Target?.Value.ToSystemNumrics() ?? Vector4.Zero;
			if (ImGui.DragFloat4((fieldName.Value ?? "null") + $"##{ReferenceID.id}", ref val, 0.1f, -10000, 10000, "%.2f", ImGuiSliderFlags.NoRoundToFormat))
			{
				if (target.Target != null)
                {
                    var casted = (Vector4f)val;
                    if (target.Target.Value != casted)
                    {
                        target.Target.Value = casted;
                    }
                }
            }
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				if (source != null)
				{
					source.Referencer.Target = target.Target;
				}
			}
			if (target.Target?.Driven ?? false)
			{
				ImGui.PopStyleColor();
			}
			if (Changeboarder)
			{
				if (ImGui.IsItemHovered() && source.DropedRef)
				{
					var e = (Sync<Vector4f>)source.HolderReferen;
					if (target.Target != null)
                    {
                        target.Target.Value = e.Value;
                    }

                    source.Referencer.Target = null;
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
			}
		}
	}
}
