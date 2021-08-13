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
    public class ImGUIBeginGroup : UIWidgetList
    {



        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);

        }

        public ImGUIBeginGroup(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIBeginGroup()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            foreach (var item in children)
            {
               item.target?.ImguiRender(imGuiRenderer);
            }
        }
    }
}

