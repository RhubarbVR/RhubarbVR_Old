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
using RhubarbEngine.Render.Material.Fields;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class MaterialFieldProperties : WorkerProperties, IPropertiesElement
	{
		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
            MaterialField materialField = null;
            try
            {
                materialField = (MaterialField)target.Target;
            }
            catch
            {
            }
			ImGui.Text(materialField?.fieldName.Value??"Null");
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
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
					source.Referencer.Target = target.Target;
				}
			}
            Helper.ThreadSafeForEach(children, (item) =>((SyncRef<IPropertiesElement>)item).Target?.ImguiRender(imGuiRenderer, canvas));
		}
	}
}
