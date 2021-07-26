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

    public class SessionsScreen : ImGUIBeginChild
    {
        public SyncRef<DashManager> dash;

        public SyncRef<ImGUIInputText> sessionId;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            dash = new SyncRef<DashManager>(this, newRefIds);
            sessionId = new SyncRef<ImGUIInputText>(this, newRefIds);
        }

        public override void OnAttach()
        {
            base.OnAttach();

            var v = entity.attachComponent<ImGUIInputText>();
            v.label.value = "SessionID";
            sessionId.target = v;
            children.Add().target = v;

            var e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Back";
            e.action.Target = Back;
            children.Add().target = e;

            e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Join";
            e.action.Target = Join;
            children.Add().target = e;

        }

        private void Join()
        {
            try
            {
                engine.worldManager.JoinSessionFromUUID(sessionId.target?.text.value);
            }
            catch
            {

            }
        }

        private void Back()
        {
            dash.target?.OpenScreen("main");
        }


        public SessionsScreen(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SessionsScreen()
        {
        }
    }
}
