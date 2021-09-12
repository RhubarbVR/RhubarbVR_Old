using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using RNumerics;

namespace RhubarbEngine.Components.ImGUI
{
	public abstract class ComponentAttacherField : UIWidget
	{
		public SyncRef<ComponentAttacher> target;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<ComponentAttacher>(this, newRefIds);
		}


		public ComponentAttacherField(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ComponentAttacherField()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
		}
	}
}