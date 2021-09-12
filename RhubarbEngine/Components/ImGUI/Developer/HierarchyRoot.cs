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


	[Category("ImGUI/Developer")]
	public class HierarchyRoot : UIWidget
	{
		public Sync<Vector2f> size;
		public Sync<bool> border;
		public Sync<ImGuiWindowFlags> windowflag;

		public SyncRef<HierarchyItem> root;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			size = new Sync<Vector2f>(this, newRefIds);
			border = new Sync<bool>(this, newRefIds);
            windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds)
            {
                Value = ImGuiWindowFlags.None
            };
            root = new SyncRef<HierarchyItem>(this, newRefIds);
		}

		public void Initialize(Entity tentity)
		{
			var e = entity.AttachComponent<HierarchyItem>();
			root.Target = e;
			e.target.Target = tentity;
		}

		public HierarchyRoot(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public HierarchyRoot()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (ImGui.BeginChild(referenceID.id.ToString(), new Vector2(size.Value.x, size.Value.y), border.Value, windowflag.Value))
			{
				root.Target?.ImguiRender(imGuiRenderer, canvas);
				ImGui.EndChild();
			}
		}
	}
}
