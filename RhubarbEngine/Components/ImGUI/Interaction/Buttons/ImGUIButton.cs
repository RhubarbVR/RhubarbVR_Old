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


	[Category("ImGUI/Interaction/Button")]
	public class ImGUIButton : UIWidget
	{

		public Sync<Vector2f> size;
		public Sync<string> label;
		public SyncDelegate action;
		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			size = new Sync<Vector2f>(this, newRefIds);
			label = new Sync<string>(this, newRefIds);
			action = new SyncDelegate(this, newRefIds);
		}

		public ImGUIButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIButton()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.Button(label.Value ?? "", new Vector2(size.Value.x, size.Value.y)))
			{
				action.Target?.Invoke();
			}
		}
	}
}
