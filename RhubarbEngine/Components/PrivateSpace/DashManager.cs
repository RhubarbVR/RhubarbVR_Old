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
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.PrivateSpace
{
    public class DashManager : Component
    {
        string screen = null;

        public SyncRef<Entity> root;
        public SyncRef<ImGUICanvas> canvas;


        public override void buildSyncObjs(bool newRefIds)
        {
            root = new SyncRef<Entity>(this, newRefIds);
            canvas = new SyncRef<ImGUICanvas>(this, newRefIds);
        }


        public override void OnAttach()
        {
            base.onLoaded();
            var e = entity.addChild("Main Panel");
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(90f, -90f, -90f);
            e.position.value = new Vector3f(0, 0, -1);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUI.ImGUICanvas>();
            //var output = e.attachComponent<Audio.AudioOutput>();
            //output.audioSource.target = imGUICanvas;
            //output.audioSource.target = audioe;
            imGUICanvas.scale.value = bmeshcol.pixelSize.value = new Vector2u(1080, 1080);
            canvas.target = imGUICanvas;
            imGUICanvas.imputPlane.target = bmeshcol;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            imGUICanvas.noCloseing.value = true;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
        }

        public void OpenScreen(string name)
        {
            if (name == screen) return;
            if(root.target != null)
            {
                logger.Log("Destroy");
                root.target.Destroy();
            }
            switch (name)
            {
                case "login":
                    root.target = entity.addChild("LoginScreen");
                    if (canvas.target != null)
                    {
                        var e = root.target.attachComponent<LoginScreen>();
                        canvas.target.element.target = e;
                        e.dash.target = this;
                    }
                    break;
                case "register":
                    root.target = entity.addChild("RegisterScreen");
                    if (canvas.target != null)
                    {
                        var e = root.target.attachComponent<RegisterScreen>();
                        canvas.target.element.target = e;
                        e.dash.target = this;
                    }
                    break;
                case "sessions":
                    root.target = entity.addChild("SessionsScreen");
                    if (canvas.target != null)
                    {
                        var e = root.target.attachComponent<SessionsScreen>();
                        canvas.target.element.target = e;
                        e.dash.target = this;
                    }
                    break;
                case "createworld":
                    root.target = entity.addChild("CreateWorldScreen");
                    if (canvas.target != null)
                    {
                        var e = root.target.attachComponent<CreateWorldScreen>();
                        canvas.target.element.target = e;
                        e.dash.target = this;
                    }
                    break;
                case "main":
                    root.target = entity.addChild("MainScreen");
                    if (canvas.target != null)
                    {
                        var e = root.target.attachComponent<MainScreen>();
                        canvas.target.element.target = e;
                        e.dash.target = this;
                    }
                    break;
                default:
                    break;
            }
            screen = name;
        }

        private DateTime opened = DateTime.UtcNow;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (!engine.netApiManager.islogin)
            {
                if(!(screen == "login" || screen == "register"))
                {
                    OpenScreen("login");
                }
            }
            else
            {
                if (!(screen == "main" || screen == "createworld" || screen == "sessions"))
                {
                    OpenScreen("main");
                }
            }
            if (DateTime.UtcNow <= opened + new TimeSpan(0, 0, 2)) return;
            if (((input.mainWindows.GetKey(Veldrid.Key.ControlLeft) || input.mainWindows.GetKey(Veldrid.Key.ControlLeft)) && input.mainWindows.GetKey(Veldrid.Key.Space)) || input.mainWindows.GetKeyDown(Veldrid.Key.Escape))
            {
                entity.enabled.value = !entity.enabled.value;
                opened = DateTime.UtcNow; 
            }
            if (input.MenuPress(Input.Creality.None))
            {
                entity.enabled.value = !entity.enabled.value;
                opened = DateTime.UtcNow;
            }
        }

        public DashManager(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public DashManager()
        {
        }
    }
}
