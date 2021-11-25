﻿using System;
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
	[Category("ImGUI/Developer/SyncMemberObservers")]
	public class NoAddSyncListObserver : SyncListBaseObserver, IPropertiesElement
	{


		public NoAddSyncListObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public NoAddSyncListObserver()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.Text(fieldName.Value ?? "NUll");
			if (ImGui.BeginChild(ReferenceID.id.ToString()))
			{
				RenderChildren(imGuiRenderer, canvas);
				ImGui.EndChild();
			}
		}

	}

}
