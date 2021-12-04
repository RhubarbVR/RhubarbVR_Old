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
using System.Threading;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class EntityProperties : WorkerProperties, IPropertiesElement
	{
		public EntityProperties(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public EntityProperties()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
            Entity entity = null;
            try
            {
                entity = (Entity)target.Target;
            }
            catch { }
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
			ImGui.Text($"{entity?.name.Value ?? "null"} ID:({target.Target?.ReferenceID.id.ToHexString() ?? "null"})");
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				if (source != null)
				{
					source.Referencer.Target = target.Target;
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("X##" + ReferenceID.id.ToString()))
			{
				var e = entity?.parent.Target;
                entity?.Destroy();
				target.Target = e;
			}
			ImGui.SameLine();
			if (ImGui.Button("+##" + ReferenceID.id.ToString()))
			{
				var e = entity?.AddChild();
				if (e != null)
                {
                    target.Target = e;
                }
            }
			ImGui.SameLine();
			if (ImGui.ArrowButton(ReferenceID.id.ToString(), ImGuiDir.Up))
			{
				var c = entity.parent.Target.AddChild(entity.name.Value + "Parent");
				if (target.Target != null)
                {
                    entity.parent.Target = c;
                }

                if (c != null)
                {
                    target.Target = c;
                }
            }
            Helper.ThreadSafeForEach(children, (item) => ((SyncRef<IPropertiesElement>)item).Target?.ImguiRender(imGuiRenderer, canvas));
			ImGui.EndChild();
			if (ImGui.IsMouseClicked(ImGuiMouseButton.COUNT))
			{
				World.lastEntityObserver = this;
			}
		}
	}
}
