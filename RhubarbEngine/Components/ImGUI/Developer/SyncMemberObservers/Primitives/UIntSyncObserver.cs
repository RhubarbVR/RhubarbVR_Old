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
	public class UIntSyncObserver : UIWidget, IPropertiesElement
	{
		public Sync<string> fieldName;

		public SyncRef<Sync<uint>> target;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<Sync<uint>>(this, newRefIds);
			fieldName = new Sync<string>(this, newRefIds);
		}


		public UIntSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public UIntSyncObserver()
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
				if (typeof(IPrimitiveEditable).IsAssignableFrom(type))
				{
					Changeboarder = true;
				}
			}
			if (Changeboarder)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
				ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
			}
			var val = target.Target?.Value ?? 0;
			if (ImGui.DragScalarN((fieldName.Value ?? "null") + $"##{ReferenceID.id}",ImGuiDataType.U32, (IntPtr)(&val), 1, 1))
			{
				if (target.Target != null)
                {
                    if (target.Target.Value != val)
                    {
                        target.Target.Value = val;
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
                    var e = (IPrimitiveEditable)source.HolderReferen;
					if (target.Target != null)
                    {
                        target.Target.PrimitiveString = e.PrimitiveString;
                    }

                    source.Referencer.Target = null;
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
			}
		}
	}
}
