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


	[Category("ImGUI/Interaction")]
	public class ImGUIColorEdit : UIWidget
	{

		public Sync<string> label;

		public Sync<Colorf> value;
		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
            label = new Sync<string>(this, newRefIds)
            {
                Value = "ColorThing"
            };
            value = new Sync<Colorf>(this, newRefIds);
		}

		public ImGUIColorEdit(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIColorEdit()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var vale = value.Value.ToRGBA().ToSystem();
			ImGui.ColorEdit4(label.Value ?? "", ref vale);
			if (vale != value.Value.ToRGBA().ToSystem())
			{
				value.Value = new Colorf(vale.X, vale.Y, vale.Z, vale.W);
			}
		}
	}
}
