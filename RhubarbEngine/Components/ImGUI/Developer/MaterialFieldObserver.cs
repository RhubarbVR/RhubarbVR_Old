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
using RhubarbEngine.Render.Material.Fields;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class MaterialFieldObserver : UIWidget, IObserver
	{

		public SyncRef<MaterialField> target;

		public SyncRef<IObserver> root;

		public SyncRefList<IObserver> children;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<MaterialField>(this, newRefIds);
			target.Changed += Target_Changed;
			root = new SyncRef<IObserver>(this, newRefIds);
			children = new SyncRefList<IObserver>(this, newRefIds);
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.Manager != world.LocalUser)
				return;
			var e = new Thread(BuildView, 1024);
			e.Priority = ThreadPriority.BelowNormal;
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
					return;
				FieldInfo[] fields = target.Target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				foreach (var field in fields)
				{
					if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute), false).Length <= 0))
					{
						var obs = entity.AttachComponent<WorkerObserver>();
						obs.fieldName.Value = field.Name;
						obs.target.Target = ((Worker)field.GetValue(target.Target));
						children.Add().Target = obs;
					}
				}
			}
			catch { }
		}


		public MaterialFieldObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MaterialFieldObserver()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.Text(target.Target.fieldName.Value);
			if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
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
					source.Referencer.Target = target.Target;
				}
			}
			foreach (var item in children)
			{
				item.Target?.ImguiRender(imGuiRenderer, canvas);
			}
		}
	}
}
