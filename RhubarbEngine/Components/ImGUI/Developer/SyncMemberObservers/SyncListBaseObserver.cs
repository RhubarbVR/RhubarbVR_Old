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
	public class SyncListBaseObserver : UIWidget, IObserver
	{
		public virtual bool removeable => true;

		public Sync<string> fieldName;

		public SyncRef<ISyncList> target;

		public SyncRefList<WorkerObserver> children;

		public SyncRef<Entity> childrenHolder;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<ISyncList>(this, newRefIds);
			target.Changed += Target_Changed;
			fieldName = new Sync<string>(this, newRefIds);
			children = new SyncRefList<WorkerObserver>(this, newRefIds);
			childrenHolder = new SyncRef<Entity>(this, newRefIds);
		}

		public override void Dispose()
		{
			base.Dispose();
			childrenHolder.Target?.Dispose();
		}

		private void Target_Changed(IChangeable obj)
		{
			if (entity.Manager != world.LocalUser)
				return;
			foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
			if (target.Target == null)
				return;
			if (childrenHolder.Target == null)
			{
				childrenHolder.Target = entity.AddChild(fieldName.Value + "Holder");
			}
			int index = 0;
			foreach (var item in target.Target)
			{
				if (typeof(Worker).IsAssignableFrom(item.GetType()))
				{
					var obs = childrenHolder.Target.AttachComponent<WorkerObserver>();
					obs.fieldName.Value = index.ToString();
					obs.target.Target = ((Worker)item);
					children.Add().Target = obs;
				}
				index++;
			}
		}

		public SyncListBaseObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SyncListBaseObserver()
		{
		}

		public virtual void ChildRender(int index, ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (children[index].Target != null)
			{
				children[index].Target.ImguiRender(imGuiRenderer, canvas);
				if (removeable)
				{
					ImGui.SameLine();
					if (ImGui.Button("X##" + referenceID.id.ToString()))
					{
						target.Target?.Remove(index);
					}
				}
			}
		}

		public virtual void RenderChildren(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			for (int i = 0; i < children.Count(); i++)
			{
				ChildRender(i, imGuiRenderer, canvas);
			}
		}
	}

}
