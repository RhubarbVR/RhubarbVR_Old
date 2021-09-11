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
using System.Threading;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class EntityObserver : UIWidget, IObserver
	{

		public SyncRef<Entity> target;

		public SyncRefList<IObserver> children;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<Entity>(this, newRefIds);
			target.Changed += Target_Changed;
			children = new SyncRefList<IObserver>(this, newRefIds);
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.manager != world.localUser)
				return;
			var e = new Thread(BuildView, 1024);
			e.Priority = ThreadPriority.BelowNormal;
			e.Start();
		}
		private void ClearOld()
		{
			foreach (var item in children)
			{
				item.target?.Dispose();
			}
			children.Clear();
		}
		public EntityObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public EntityObserver()
		{
		}
		[NoSave]
		[NoShow]
		[NoSync]
		Entity e;

		private void BuildView()
		{
			try
			{
				ClearOld();
				if (target.target == null)
					return;
				FieldInfo[] fields = typeof(Entity).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				//This is a temp fix
				if (e == null)
				{
					e = entity.addChild("Entity Children");
					e.persistence.value = false;
				}
				//I should remove on change update before initialized or add a on initialized check inside this function
				foreach (var field in fields)
				{
					if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute), false).Length <= 0))
					{
						var obs = e.attachComponent<WorkerObserver>();
						obs.fieldName.value = field.Name;
						obs.target.target = ((Worker)field.GetValue(target.target));
						children.Add().target = obs;
					}
				}
			}
			catch { }
		}

		public override void Dispose()
		{
			base.Dispose();
			if (world.lastEntityObserver == this)
			{
				world.lastEntityObserver = null;
			}
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
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
			ImGui.Text($"{target.target?.name.value ?? "null"} ID:({target.target?.referenceID.id.ToHexString() ?? "null"})");
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				if (source != null)
				{
					source.Referencer.target = target.target;
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("X##" + referenceID.id.ToString()))
			{
				var e = target.target?.parent.target;
				target.target?.Destroy();
				target.target = e;
			}
			ImGui.SameLine();
			if (ImGui.Button("+##" + referenceID.id.ToString()))
			{
				var e = target.target?.addChild();
				if (e != null)
					target.target = e;
			}
			ImGui.SameLine();
			if (ImGui.ArrowButton(referenceID.id.ToString(), ImGuiDir.Up))
			{
				var c = target.target.parent.target.addChild(target.target.name.value + "Parent");
				if (target.target != null)
					target.target.parent.target = c;
				if (c != null)
					target.target = c;
			}
			foreach (var item in children)
			{
				item.target?.ImguiRender(imGuiRenderer, canvas);
			}
			ImGui.EndChild();
			if (ImGui.IsMouseClicked(ImGuiMouseButton.COUNT))
			{
				world.lastEntityObserver = this;
			}
		}
	}
}
