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
	public class ImGUIBeginCombo : UIWidgetList
	{
		public Sync<string> label;

		public Sync<string> preview;

		public Sync<ImGuiComboFlags> comboflag;


		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			label = new Sync<string>(this, newRefIds);
			preview = new Sync<string>(this, newRefIds);
            comboflag = new Sync<ImGuiComboFlags>(this, newRefIds)
            {
                Value = ImGuiComboFlags.None
            };
        }

		public ImGUIBeginCombo(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIBeginCombo()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.BeginCombo(label.Value ?? "", preview.Value ?? "", comboflag.Value))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.EndCombo();
			}
		}
	}
}
