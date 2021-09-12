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
	public class ImGUIRadioButton : UIWidget
	{
		public Sync<string> id;
		public Sync<bool> active;

		public SyncDelegate action;
		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			id = new Sync<string>(this, newRefIds);
			active = new Sync<bool>(this, newRefIds);
			action = new SyncDelegate(this, newRefIds);
		}

		public ImGUIRadioButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public ImGUIRadioButton()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.RadioButton(id.Value ?? "", active.Value))
			{
				action.Target?.Invoke();
			}
		}
	}
}
