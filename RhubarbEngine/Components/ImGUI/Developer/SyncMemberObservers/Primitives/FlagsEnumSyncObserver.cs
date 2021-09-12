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
	public class FlagsEnumSyncObserver : UIWidget, IObserver
	{
		public Sync<string> fieldName;

		public SyncRef<IPrimitiveEditable> target;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			target = new SyncRef<IPrimitiveEditable>(this, newRefIds);
			fieldName = new Sync<string>(this, newRefIds);
		}


		public FlagsEnumSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public FlagsEnumSyncObserver()
		{
		}
	}

	[Category("ImGUI/Developer/SyncMemberObservers/Primitives")]
	public class FlagsEnumSyncObserver<T> : FlagsEnumSyncObserver, IObserver where T : struct, System.Enum
	{

		public FlagsEnumSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public FlagsEnumSyncObserver()
		{
		}

		string[] ve = Enum.GetNames(typeof(T));

		public unsafe override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (target.Target?.Driven ?? false)
			{
				var e = ImGui.GetStyleColorVec4(ImGuiCol.FrameBg);
				var vec = (Vector4f)(*e);
				ImGui.PushStyleColor(ImGuiCol.FrameBg, (vec - new Vector4f(0, 0.5f, 0, 0)).ToSystem());
			}
			int c = Array.IndexOf(ve, Enum.GetName(typeof(T), (((Sync<T>)target.Target).Value)));
			ImGui.Combo((fieldName.Value ?? "null") + $"##{referenceID.id}", ref c, ve, ve.Length);
			if (c != (int)(object)(((Sync<T>)target.Target).Value))
			{
				((Sync<T>)target.Target).Value = Enum.GetValues<T>()[c];
			}
			if (target.Target?.Driven ?? false)
			{
				ImGui.PopStyleColor();
			}
		}
	}
}
