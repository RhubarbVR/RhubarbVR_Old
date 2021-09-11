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


	[Category("ImGUI/Interaction/Button")]
	public class ImGUIArrowButton : UIWidget
	{

		public Sync<ImGuiDir> imGuiDir;
		public Sync<string> id;
		public SyncDelegate action;
		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			imGuiDir = new Sync<ImGuiDir>(this, newRefIds);
			imGuiDir.value = ImGuiDir.None;
			id = new Sync<string>(this, newRefIds);
			action = new SyncDelegate(this, newRefIds);
		}

		public ImGUIArrowButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIArrowButton()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.ArrowButton(id.value ?? "", imGuiDir.value))
			{
				action.Target?.Invoke();
			}
		}
	}
}
