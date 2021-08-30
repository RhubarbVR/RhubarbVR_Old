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
            windowflag = new Sync<ImGuiWindowFlags>(this, newRefIds);
            windowflag.value = ImGuiWindowFlags.None;
            root = new SyncRef<HierarchyItem>(this, newRefIds);
        }

        public void Initialize(Entity tentity)
        {
            var e =  entity.attachComponent<HierarchyItem>();
            root.target = e;
            e.target.target = tentity;
        }

        public HierarchyRoot(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public HierarchyRoot()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            if (ImGui.BeginChild(referenceID.id.ToString(), new Vector2(size.value.x, size.value.y), border.value, windowflag.value))
            {
                root.target?.ImguiRender(imGuiRenderer);
                ImGui.EndChild();
            }
        }
    }
}
