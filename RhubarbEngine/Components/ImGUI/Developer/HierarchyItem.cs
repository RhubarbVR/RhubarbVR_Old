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
		private Entity last;

		public SyncRefList<HierarchyItem> children;

		private bool Bound;

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
			if (entity.manager != world.localUser)
				return;
			if (dropedDown.value)
			{
				BuildChildren();
			}
		}

		private void BuildChildren()
		{
			foreach (var item in children)
			{
				item.target?.Dispose();
			}
			children.Clear();
			if (target.target == null)
				return;
			int index = 0;
			foreach (var item in target.target._children)
			{
				var newHierarchyItem = entity.attachComponent<HierarchyItem>();
				children.Add().target = newHierarchyItem;
				newHierarchyItem.target.target = item;
				index++;
			}
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.manager != world.localUser)
				return;
			Bind();
			last = target.target;
		}

		private void Bind()
		{
			if (entity.manager != world.localUser)
				return;
			if (Bound)
			{

				Bound = false;
			}


		}

		private void UnBind()
		{
			if (Bound)
			{

				Bound = false;
			}

		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (entity.manager == world.localUser)
				return;
			if (Bound)
			{
				UnBind();
				Bound = false;
			}
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			bool val = dropedDown.value;
			ImGui.SetNextItemOpen(val);
			if (ImGui.TreeNodeEx($"{target.target?.name.value ?? "null"}##{referenceID.id.ToString()}", ImGuiTreeNodeFlags.OpenOnArrow))
			{
				foreach (var item in children)
				{
					item.target?.ImguiRender(imGuiRenderer, canvas);
				}
				if (!val)
				{
					dropedDown.value = true;
				}
				ImGui.TreePop();
			}
			else
			{
				if (val)
				{
					dropedDown.value = false;
				}
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
			if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
			{
				Clicked();
			}
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				Grabbed(source, canvas);
			}
			if (ImGui.IsItemHovered() && source.DropedRef)
			{
				if (typeof(Entity) == source.Referencer.target.GetType())
				{
					((Entity)source.Referencer.target).parent.target = target.target;
				}
			}
		}

		private void Clicked()
		{
			if (world.lastEntityObserver != null)
				world.lastEntityObserver.target.target = target.target;
		}
		private void Grabbed(Interaction.GrabbableHolder source, ImGUICanvas canvas)
		{
			if (source != null)
			{
				if (source.Referencer.target == null)
				{
					source.Referencer.target = target.target;
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
