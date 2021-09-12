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
	public class ImGUIBeginChild : UIWidgetList
	{
		public Sync<string> id;

		public Sync<Vector2f> size;
		public Sync<bool> border;
		public Sync<ImGuiWindowFlags> windowflag;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			id = new Sync<string>(this, newRefIds);
			size = new Sync<Vector2f>(this, newRefIds);
			border = new Sync<bool>(this, newRefIds);
			windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
			windowflag.Value = ImGuiWindowFlags.None;
		}

		public ImGUIBeginChild(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginChild()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.BeginChild(id.Value ?? "", new Vector2(size.Value.x, size.Value.y), border.Value, windowflag.Value))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.EndChild();
			}
		}
	}
}
