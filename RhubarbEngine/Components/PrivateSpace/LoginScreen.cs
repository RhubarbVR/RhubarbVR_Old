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

    public class LoginScreen : ImGUIBeginChild
    {

        public SyncRef<ImGUIInputText> email;
        public SyncRef<ImGUIInputText> password;
        public SyncRef<ImGUIButton> loginButton;
        public SyncRef<ImGUIButton> registerButton;
        public SyncRef<ImGUICheckBox> rememberMe;
        public SyncRef<DashManager> dash;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            email = new SyncRef<ImGUIInputText>(this, newRefIds);
            password = new SyncRef<ImGUIInputText>(this, newRefIds);
            loginButton = new SyncRef<ImGUIButton>(this, newRefIds);
            registerButton = new SyncRef<ImGUIButton>(this, newRefIds);
            rememberMe = new SyncRef<ImGUICheckBox>(this, newRefIds);
            dash = new SyncRef<DashManager>(this, newRefIds);

        }

        public override void OnAttach()
        {
            base.OnAttach();
            var l = entity.attachComponent<ImGUIInputText>();
            l.label.value = "Email";
            email.target = l;
            children.Add().target = l;
            l = entity.attachComponent<ImGUIInputText>();
            l.label.value = "Password";
            l.flags.value |= ImGuiInputTextFlags.Password;
            password.target = l;
            children.Add().target = l;
            var m = entity.attachComponent<ImGUICheckBox>();
            m.label.value = "Remember Me";
            rememberMe.target = m;
            children.Add().target = m;
            var e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Login";
            e.action.Target = Login;
            loginButton.target = e;
            children.Add().target = e;
            e = entity.attachComponent<ImGUIButton>();
            e.label.value = "Register";
            e.action.Target = Register;
            registerButton.target = e;
            children.Add().target = e;

        }

        public void Login()
        {
            try
            {
                engine.netApiManager.login(email.target?.text.value, password.target?.text.value, rememberMe.target?.value.value??false);
            }
            catch(Exception e)
            {
                logger.Log("Failed to Login:" + e.ToString(), true);
            }
        }

        public void Register()
        {
            dash.target?.OpenScreen("register");
        }

        public LoginScreen(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public LoginScreen()
        {
        }
    }
}
