using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using RNumerics;
using ImGuiNET;
using System.Numerics;
using System.Linq;

namespace RhubarbEngine.Components.ImGUI
{

	[Category("ImGUI/Developer")]
	public class ComponentAttacherAttach : ComponentAttacherField
	{
		public Sync<string> type;

		private Type _setType;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			type = new Sync<string>(this, newRefIds);
			type.Changed += Type_Changed;
		}

		private void Type_Changed(IChangeable obj)
		{
			_setType = Type.GetType(type.Value);
		}


		public ComponentAttacherAttach(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ComponentAttacherAttach()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.PushStyleColor(ImGuiCol.Button, (Vector4)Colorf.DarkBlue.ToRGBA());
			if (ImGui.Button(_setType.GetFormattedName() ?? "Null" + "##" + referenceID.id, new Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
			{
				if ((target.Target != null) && (_setType != null))
                {
                    target.Target.AttachComponent(_setType);
                }
            }
			ImGui.PopStyleColor();
		}
	}
}