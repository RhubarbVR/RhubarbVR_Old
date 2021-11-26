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
	[Category("ImGUI/Developer/SyncMemberObservers")]
	public class SyncRefObserver<T> : SyncRefObserver, IPropertiesElement where T : class, IWorldObject
	{

		public SyncRefObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SyncRefObserver()
		{
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
				if (typeof(T).IsAssignableFrom(type))
				{
					Changeboarder = true;
				}
				else if (typeof(SyncRef<T>).IsAssignableFrom(type))
				{
					Changeboarder = true;
				}
			}
			if (Changeboarder)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3);
				ImGui.PushStyleColor(ImGuiCol.Border, Colorf.BlueMetal.ToRGBA().ToSystem());
			}
			var val = "null";
			if (target.Target != null)
			{
				val = $"{target.Target?.TargetIWorldObject.GetNameString() ?? ""} ID:({target.Target?.TargetIWorldObject?.ReferenceID.id.ToHexString() ?? "null"})";
			}
			if (ImGui.Button("^##" + ReferenceID.id.ToString()))
			{
				target.Target.TargetIWorldObject?.OpenWindow();
			}
			ImGui.SameLine();
			if (ImGui.Button("X##" + ReferenceID.id.ToString()))
			{
				if (target.Target != null)
                {
                    target.Target.Value = default;
                }
            }
			ImGui.SameLine();
			ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - 45);
			ImGui.InputText((fieldName.Value ?? "null") + $"##{ReferenceID.id}", ref val, (uint)val.Length + 255, ImGuiInputTextFlags.ReadOnly);
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
					var type = source.HolderReferen?.GetType();
					if (typeof(T).IsAssignableFrom(type))
					{
						if (target.Target != null)
                        {
                            target.Target.TargetIWorldObject = source.HolderReferen;
                        }

                        source.Referencer.Target = null;
					}

					else if (typeof(SyncRef<T>).IsAssignableFrom(type))
					{
						if (target.Target != null)
                        {
                            target.Target.TargetIWorldObject = ((SyncRef<T>)source.HolderReferen).Target;
                        }

                        source.Referencer.Target = null;
					}
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
			}
		}
	}



	public class SyncRefObserver : UIWidget, IPropertiesElement
	{
		public Sync<string> fieldName;

		public SyncRef<ISyncRef> target;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
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
