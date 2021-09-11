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
	[Category(new string[] { "ImGUI" })]
	public abstract class UIWidget : Component, IUIElement
	{
		public UIWidget(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public UIWidget()
		{
		}

		public virtual void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
		}
	}
}
