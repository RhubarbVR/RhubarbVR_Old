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
    [Category("ImGUI/Developer/SyncMemberObservers")]
    public class SyncComponentListObserver : SyncListBaseObserver, IObserver
    {
        public override bool removeable => false;

        public SyncComponentListObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncComponentListObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            
            if (ImGui.BeginChild(referenceID.id.ToString(),new Vector2(ImGui.GetWindowContentRegionWidth(), ImGui.GetFrameHeightWithSpacing() - 50f)))
            {
                RenderChildren(imGuiRenderer,canvas);
                ImGui.EndChild();
            }
            if (ImGui.Button($"Attach Component##{referenceID.id}",new Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
            {
                World.World createWorld = world.worldManager.focusedWorld ?? world;
                Entity User = createWorld.userRoot.entity;
                Entity par = User.parent.target;
                var (cube, win, comp) = Helpers.MeshHelper.attachWindow<ComponentAttacher>(par);
                var headPos = createWorld.userRoot.Headpos;
                var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
                cube.setGlobalTrans(move * headPos);
                comp.Tentity.target = target.target.getClosedEntity();
            }
        }

    }

}
