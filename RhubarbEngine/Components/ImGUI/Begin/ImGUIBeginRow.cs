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


	[Category("ImGUI/Begin")]
	public class ImGUIBeginRow : UIWidgetList
	{


		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
		}

		public ImGUIBeginRow(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginRow()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.Columns(children.Count());
			foreach (var item in children)
			{
				item.target?.ImguiRender(imGuiRenderer, canvas);
				ImGui.NextColumn();
			}
		}
	}
}
