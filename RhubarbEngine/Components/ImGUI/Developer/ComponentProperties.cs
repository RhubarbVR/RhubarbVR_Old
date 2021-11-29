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
	public class ComponentProperties : UIWidget, IPropertiesElement
	{

		public SyncRef<Component> target;

		public SyncRef<IPropertiesElement> root;

		public SyncRefList<IPropertiesElement> children;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<Component>(this, newRefIds);
			target.Changed += Target_Changed;
			root = new SyncRef<IPropertiesElement>(this, newRefIds);
			children = new SyncRefList<IPropertiesElement>(this, newRefIds);
		}

		private void Target_Changed(IChangeable obj)
		{
			if (Entity.Manager != World.LocalUser)
            {
                return;
            }

            var e = new Thread(BuildView, 1024)
            {
                Priority = ThreadPriority.BelowNormal
            };
            e.Start();
		}

		private void ClearOld()
		{
			foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
		}

		private void BuildView()
		{
			try
			{
				ClearOld();
				if (target.Target == null)
                {
                    return;
                }

                var fields = target.Target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				foreach (var field in fields)
				{
					if (typeof(IWorker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute), false).Length <= 0))
					{
						var obs = Entity.AttachComponent<WorkerProperties>();
						obs.fieldName.Value = field.Name;
                        obs.target.Target = (IWorker)field.GetValue(target.Target);
						children.Add().Target = obs;
					}
				}
			}
			catch { }
		}


		public ComponentProperties(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ComponentProperties()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var open = true;
			Vector2 max;
			Vector2 min;
			if (ImGui.CollapsingHeader($"{target.Target?.GetType().GetFormattedName() ?? "null"} ID:({target.Target?.ReferenceID.id.ToHexString() ?? "null"}) ##{ReferenceID.id}", ref open))
			{
				max = ImGui.GetItemRectMax();
				min = ImGui.GetItemRectMin();
                Helper.ThreadSafeForEach(children, (item) => ((SyncRef<IPropertiesElement>)item).Target?.ImguiRender(imGuiRenderer, canvas));
			}
			else
			{
				max = ImGui.GetItemRectMax();
				min = ImGui.GetItemRectMin();
			}
			if (ImGui.IsMouseHoveringRect(min, max) && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
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
			if (!open)
			{
				target.Target?.Dispose();
			}

		}
	}
}
