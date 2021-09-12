using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using g3;
using ImGuiNET;
using System.Collections.Generic;

namespace RhubarbEngine.Components.ImGUI
{

	[Category("ImGUI/Developer")]
	public class ComponentAttacherPath : ComponentAttacherField
	{
		public Sync<string> path;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			path = new Sync<string>(this, newRefIds);
		}


		public ComponentAttacherPath(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ComponentAttacherPath()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.Button(path.Value + "##" + referenceID.id, new System.Numerics.Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
			{
				if (path.Value == "../")
				{
					if (target.Target != null)
					{
						string news = "/";
						string temp = "";
						foreach (var item in target.Target.path.Value.Split('/', '\\'))
						{
							if (!string.IsNullOrEmpty(item))
							{
								news += temp;
								temp = item;
							}
						}
						if (target.Target.path.Value.Contains("`1"))
						{
							target.Target.path.Value = target.Target.path.Value.Replace("`1", "");
						}
						else
						{
							target.Target.path.Value = news;
						}
					}
				}
				else
				{
					if (target.Target != null)
						target.Target.path.Value += path.Value;
				}
			}
		}
	}
}