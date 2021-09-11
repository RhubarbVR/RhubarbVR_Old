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
	public class ImGUIBeginTooltip : UIWidgetList
	{
		public Sync<string> id;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			id = new Sync<string>(this, newRefIds);

		}

		public ImGUIBeginTooltip(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginTooltip()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.BeginTooltip();
			foreach (var item in children)
			{
				item.target?.ImguiRender(imGuiRenderer, canvas);
			}
			ImGui.EndTooltip();
		}

	}
}

