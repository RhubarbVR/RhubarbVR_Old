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


	[Category("ImGUI/Begin")]
	public class ImGUIBegin : UIWidgetList
	{
		public Sync<string> name;
		public Sync<ImGuiWindowFlags> windowflag;
		public Sync<bool> open;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			name = new Sync<string>(this, newRefIds);
			windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
			windowflag.Value = ImGuiWindowFlags.None;
			open = new Sync<bool>(this, newRefIds);
		}

		public ImGUIBegin(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBegin()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			bool lopen = open.Value;
			if (ImGui.Begin(name.Value ?? "", ref lopen, windowflag.Value))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.End();
			}
			if (lopen != open.Value)
			{
				open.Value = lopen;
			}
		}
	}
}
