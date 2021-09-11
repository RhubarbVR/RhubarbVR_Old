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
	[Category("ImGUI/Developer/SyncMemberObservers")]
	public class NoAddSyncListObserver : SyncListBaseObserver, IObserver
	{


		public NoAddSyncListObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public NoAddSyncListObserver()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.Text(fieldName.value ?? "NUll");
			if (ImGui.BeginChild(referenceID.id.ToString()))
			{
				RenderChildren(imGuiRenderer, canvas);
				ImGui.EndChild();
			}
		}

	}

}
