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


	[Category("ImGUI/Text")]
	public class ImGUIText : UIWidget
	{
		public enum TextType
		{
			Unformatted,
			Normal,
			Disabled,
			Bullet,
			Wrapped,
			LogText,
		}

		public Sync<string> text;
		public Sync<TextType> textType;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			text = new Sync<string>(this, newRefIds);
			textType = new Sync<TextType>(this, newRefIds);
			textType.Value = TextType.Normal;
		}

		public ImGUIText(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIText()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			switch (textType.Value)
			{
				case TextType.Normal:
					ImGui.Text(text.Value ?? "");
					break;
				case TextType.Disabled:
					ImGui.TextDisabled(text.Value ?? "");
					break;
				case TextType.Bullet:
					ImGui.BulletText(text.Value ?? "");
					break;
				case TextType.Wrapped:
					ImGui.TextWrapped(text.Value ?? "");
					break;
				case TextType.LogText:
					ImGui.LogText(text.Value ?? "");
					break;
				default:
					ImGui.TextUnformatted(text.Value ?? "");
					break;
			}
		}
	}
}
