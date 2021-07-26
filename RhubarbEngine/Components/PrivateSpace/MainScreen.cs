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
using RhubarbEngine.Components.ImGUI;
namespace RhubarbEngine.Components.PrivateSpace
{

    public class MainScreen : ImGUIBeginChild
    {
        public SyncRef<DashManager> dash;
        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            dash = new SyncRef<DashManager>(this, newRefIds);
        }

        public override void OnAttach()
        {
            base.OnAttach();
            var t = entity.attachComponent<ImGUIText>();
            t.text.value = "Welcome: "+engine.netApiManager.user.Username;
            children.Add().target = t;
            t = entity.attachComponent<ImGUIText>();
            t.text.value = "This is Pre Pre Pre Alpha";
            children.Add().target = t;
            var e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Create World";
            e.action.Target = CreateWorld;
            children.Add().target = e;
            e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Sessions";
            e.action.Target = Sessions;
            children.Add().target = e;
            e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Logout";
            e.action.Target = Logout;
            children.Add().target = e;
        }

        private void Logout()
        {
            engine.netApiManager.logout();
        }

        private void Sessions()
        {
            dash.target?.OpenScreen("sessions");
        }

        private void CreateWorld()
        {
            dash.target?.OpenScreen("createworld");
        }

        public MainScreen(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public MainScreen()
        {
        }
    }
}
