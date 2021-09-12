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


	[Category("ImGUI/Developer")]
	public class HierarchyItem : UIWidget
	{
		public Sync<bool> dropedDown;

		public SyncRef<Entity> target;

		[NoShow]
		[NoSave]
		[NoSync]
		private Entity _last;

		public SyncRefList<HierarchyItem> children;

		private bool _bound;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			dropedDown = new Sync<bool>(this, newRefIds);
			dropedDown.Changed += DropedDown_Changed;
			target = new SyncRef<Entity>(this, newRefIds);
			target.Changed += Target_Changed;
			children = new SyncRefList<HierarchyItem>(this, newRefIds);
		}

		private void DropedDown_Changed(IChangeable obj)
		{
			if (entity.Manager != world.LocalUser)
            {
                return;
            }

            if (dropedDown.Value)
			{
				BuildChildren();
			}
		}

		private void BuildChildren()
		{
			foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
			if (target.Target == null)
            {
                return;
            }

            var index = 0;
			foreach (var item in target.Target._children)
			{
				var newHierarchyItem = entity.AttachComponent<HierarchyItem>();
				children.Add().Target = newHierarchyItem;
				newHierarchyItem.target.Target = item;
				index++;
			}
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.Manager != world.LocalUser)
            {
                return;
            }

            Bind();
			_last = target.Target;
		}

		private void Bind()
		{
			if (entity.Manager != world.LocalUser)
            {
                return;
            }

            if (_bound)
			{

				_bound = false;
			}


		}

		private void UnBind()
		{
			if (_bound)
			{

				_bound = false;
			}

		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (entity.Manager == world.LocalUser)
            {
                return;
            }

            if (_bound)
			{
				UnBind();
				_bound = false;
			}
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var val = dropedDown.Value;
			ImGui.SetNextItemOpen(val);
			if (ImGui.TreeNodeEx($"{target.Target?.name.Value ?? "null"}##{referenceID.id.ToString()}", ImGuiTreeNodeFlags.OpenOnArrow))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				if (!val)
				{
					dropedDown.Value = true;
				}
				ImGui.TreePop();
			}
			else
			{
				if (val)
				{
					dropedDown.Value = false;
				}
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
			if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
			{
				Clicked();
			}
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				Grabbed(source);
			}
			if (ImGui.IsItemHovered() && source.DropedRef)
			{
				if (typeof(Entity) == source.Referencer.Target.GetType())
				{
					((Entity)source.Referencer.Target).parent.Target = target.Target;
				}
			}
		}

		private void Clicked()
		{
			if (world.lastEntityObserver != null)
            {
                world.lastEntityObserver.target.Target = target.Target;
            }
        }
		private void Grabbed(Interaction.GrabbableHolder source)
		{
			if (source != null)
			{
				if (source.Referencer.Target == null)
				{
					source.Referencer.Target = target.Target;
				}
			}
        }
		public HierarchyItem(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public HierarchyItem()
		{
		}

	}
}
