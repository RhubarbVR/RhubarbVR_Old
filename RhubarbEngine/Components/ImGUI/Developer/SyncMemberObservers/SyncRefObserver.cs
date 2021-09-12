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
	public class SyncRefObserver<T> : SyncRefObserver, IObserver where T : class, IWorldObject
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
			switch (canvas.imputPlane.Target?.source ?? Interaction.InteractionSource.None)
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
				var type = source.Referencer.Target?.GetType();
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
			if (ImGui.Button("^##" + referenceID.id.ToString()))
			{
				target.Target.TargetIWorldObject?.OpenWindow();
			}
			ImGui.SameLine();
			if (ImGui.Button("X##" + referenceID.id.ToString()))
			{
				if (target.Target != null)
                {
                    target.Target.Value = default;
                }
            }
			ImGui.SameLine();
			ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - 45);
			ImGui.InputText((fieldName.Value ?? "null") + $"##{referenceID.id}", ref val, (uint)val.Length + 255, ImGuiInputTextFlags.ReadOnly);
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
					var type = source.Referencer.Target?.GetType();
					if (typeof(T).IsAssignableFrom(type))
					{
						if (target.Target != null)
                        {
                            target.Target.TargetIWorldObject = source.Referencer.Target;
                        }

                        source.Referencer.Target = null;
					}

					else if (typeof(SyncRef<T>).IsAssignableFrom(type))
					{
						if (target.Target != null)
                        {
                            target.Target.TargetIWorldObject = ((SyncRef<T>)source.Referencer.Target).Target;
                        }

                        source.Referencer.Target = null;
					}
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
