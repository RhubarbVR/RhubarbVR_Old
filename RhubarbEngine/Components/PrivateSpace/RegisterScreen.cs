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

    public class RegisterScreen : ImGUIBeginChild
    {
        public SyncRef<ImGUIInputText> email;
        public SyncRef<ImGUIInputText> password;
        public SyncRef<ImGUIInputText> username;
        public SyncRef<ImGUIButton> loginButton;
        public SyncRef<ImGUIButton> registerButton;
        public SyncRef<DashManager> dash;
        public SyncRef<ImGUIint> day;
        public SyncRef<ImGUIint> year;
        public SyncRef<ImGUIint> month;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            email = new SyncRef<ImGUIInputText>(this, newRefIds);
            password = new SyncRef<ImGUIInputText>(this, newRefIds);
            loginButton = new SyncRef<ImGUIButton>(this, newRefIds);
            registerButton = new SyncRef<ImGUIButton>(this, newRefIds);
            dash = new SyncRef<DashManager>(this, newRefIds);
            username = new SyncRef<ImGUIInputText>(this, newRefIds);
            day = new SyncRef<ImGUIint>(this, newRefIds);
            year = new SyncRef<ImGUIint>(this, newRefIds);
            month = new SyncRef<ImGUIint>(this, newRefIds);
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
            l = entity.attachComponent<ImGUIInputText>();
            l.label.value = "Username";
            password.target = l;
            children.Add().target = l;


            var intv = entity.attachComponent<ImGUIint>();
            intv.label.value = "Birth Day";
            intv.value.value = DateTime.UtcNow.Day;
            day.target = intv;
            children.Add().target = intv;

            intv = entity.attachComponent<ImGUIint>();
            intv.label.value = "Birth Year";
            intv.value.value = DateTime.UtcNow.Year;
            year.target = intv;
            children.Add().target = intv;

            intv = entity.attachComponent<ImGUIint>();
            intv.label.value = "Birth Month";
            intv.value.value = DateTime.UtcNow.Month;
            month.target = intv;
            children.Add().target = intv;

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

        private void Login()
        {
            dash.target?.OpenScreen("login");
        }

        private void Register()
        {
            try
            {
                engine.netApiManager.register(email.target?.text.value, password.target?.text.value, username.target?.text.value, new DateTime(year.target?.value.value??9999, month.target?.value.value ?? 12, day.target?.value.value ?? 31));
            }
            catch (Exception e)
            {
                logger.Log("Failed to Register:" + e.ToString(), true);
            }
        }

        public RegisterScreen(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public RegisterScreen()
        {
        }
    }
}
