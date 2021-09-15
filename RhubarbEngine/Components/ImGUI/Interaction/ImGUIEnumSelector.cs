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
	public class ImGUIEnumSelector<T> : UIWidget where T : System.Enum
	{

		public Sync<string> label;

		public Sync<T> value;
		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			label = new Sync<string>(this, newRefIds);
			value = new Sync<T>(this, newRefIds);
		}

		public ImGUIEnumSelector(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIEnumSelector()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var c = (int)(object)value.Value;
			var e = Enum.GetNames(typeof(T)).ToList();
			ImGui.Combo(label.Value ?? "", ref c, e.ToArray(), e.Count);
			if (c != (int)(object)value.Value)
			{
				value.Value = (T)(object)c;
			}
		}
	}
}
