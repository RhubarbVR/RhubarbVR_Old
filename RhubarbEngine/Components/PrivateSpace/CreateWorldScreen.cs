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
using Org.OpenAPITools.Model;

namespace RhubarbEngine.Components.PrivateSpace
{

    public class CreateWorldScreen : ImGUIBeginChild
    {
        public SyncRef<DashManager> dash;

        public SyncRef<ImGUIEnumSelector<AccessLevel>> accessLevel;
        public SyncRef<ImGUIEnumSelector<SessionsType>> sessionsType;
        public SyncRef<ImGUIInputText> name;
        public SyncRef<ImGUIint> maxUsers;


        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            dash = new SyncRef<DashManager>(this, newRefIds);
            accessLevel = new SyncRef<ImGUIEnumSelector<AccessLevel>>(this, newRefIds);
            sessionsType = new SyncRef<ImGUIEnumSelector<SessionsType>>(this, newRefIds);
            name = new SyncRef<ImGUIInputText>(this, newRefIds);
            maxUsers = new SyncRef<ImGUIint>(this, newRefIds);
        }

        public override void OnAttach()
        {
            base.OnAttach();
            var ename = entity.attachComponent<ImGUIInputText>();
            ename.label.value = "Session Name";
            ename.text.value = engine.netApiManager.user.Username + " Session";
            name.target = ename;
            children.Add().target = ename;

            var intv = entity.attachComponent<ImGUIint>();
            intv.label.value = "Max Users";
            intv.value.value = 16;
            maxUsers.target = intv;
            children.Add().target = intv;

            var AccessLevelenums = entity.attachComponent<ImGUIEnumSelector<AccessLevel>>();
            AccessLevelenums.label.value = "Access Level";
            AccessLevelenums.value.value = AccessLevel.Anyone;
            accessLevel.target = AccessLevelenums;
            children.Add().target = AccessLevelenums;

            var SessionsTypeenums = entity.attachComponent<ImGUIEnumSelector<SessionsType>>();
            SessionsTypeenums.label.value = "Sessions Type";
            SessionsTypeenums.value.value = SessionsType.Casual;
            sessionsType.target = SessionsTypeenums;
            children.Add().target = SessionsTypeenums;


            var e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Create World";
            e.action.Target = CreateWorld;
            children.Add().target = e;
            e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Back";
            e.action.Target = Back;
            children.Add().target = e;
        }

        private void CreateWorld()
        {
           world.worldManager.createNewWorld(accessLevel.target?.value.value??AccessLevel.Anyone,sessionsType.target?.value.value??SessionsType.Casual,name.target?.text.value??"",null,false, maxUsers.target?.value.value??16, false,"Basic");
        }

        private void Back()
        {
            dash.target?.OpenScreen("main");
        }

        public CreateWorldScreen(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public CreateWorldScreen()
        {
        }
    }
}
