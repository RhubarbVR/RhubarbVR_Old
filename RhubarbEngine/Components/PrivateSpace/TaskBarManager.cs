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
    public class TaskBarManager : Component
    {
        public SyncRef<Entity> root;
        public SyncRef<Entity> startMenu;
        public SyncRef<ImGUICanvas> taskbarcanvas;
        public SyncRef<ImGUICanvas> startcanvas;
        public Driver<string> dateTextDriver;

        public override void buildSyncObjs(bool newRefIds)
        {
            root = new SyncRef<Entity>(this, newRefIds);
            startMenu = new SyncRef<Entity>(this, newRefIds);
            taskbarcanvas = new SyncRef<ImGUICanvas>(this, newRefIds);
            startcanvas = new SyncRef<ImGUICanvas>(this, newRefIds);
            dateTextDriver = new Driver<string>(this, newRefIds);
        }

        public void startMenuClick()
        {
            if (startMenu.target == null) return;
            startMenu.target.enabled.value = !startMenu.target.enabled.value;
        }

        private void startMenuFocusLost()
        {
            if (startMenu.target == null) return;
            if (startMenu.target.enabled.value)
            {
                startMenu.target.enabled.value = false;
            }
        }

        private void buildStartMenu(Entity e)
        {
            startMenu.target = e;
            BasicUnlitShader shader = world.staticAssets.basicUnlitShader;
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            bmesh.Height.value = 0.30f;
            bmesh.Width.value = 0.30f;
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //bmeshcol.onFocusLost.Target = startMenuFocusLost;
            bmeshcol.size.value = (new Vector2f(bmesh.Width.value, bmesh.Height.value))/2;
            //InputPlane bmeshcol = e.attachComponent<InputPlane>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
            e.position.value = new Vector3f(-0.5, 0.05, 0.25);
            imGUICanvas.scale.value = bmeshcol.pixelSize.value = new Vector2u(150, 150);
            imGUICanvas.imputPlane.target = bmeshcol;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            imGUICanvas.noCloseing.value = true;
            imGUICanvas.noBackground.value = true;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            startcanvas.target = imGUICanvas;
            var group = e.attachComponent<ImGUIBeginGroup>();
            var createCube = e.attachComponent<ImGUIButton>();
            var createWindow = e.attachComponent<ImGUIButton>();
            createCube.label.value = "Create Cube";
            createCube.action.Target = CreateCube;
            imGUICanvas.element.target = group;
            group.children.Add().target = createCube;

            createWindow.label.value = "Create Window";
            createWindow.action.Target = CreateWindow;
            group.children.Add().target = createWindow;

            e.enabled.value = false;
        }

        private void CreateWindow()
        {
            logger.Log("Create Window");
            World.World createWorld = world.worldManager.focusedWorld ?? world;
            Entity User = createWorld.userRoot.entity;
            Entity par = User.parent.target;
            var (cube, win,comp) = Helpers.MeshHelper.attachWindow<ImGUIInputText>(par);
            var headPos = createWorld.userRoot.Headpos;
            var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f))*Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f,-90f,0f).ToSystemNumric());
            cube.setGlobalTrans(move * headPos);

        }

        private void CreateCube()
        {
            logger.Log("Create Cube");
            World.World createWorld = world.worldManager.focusedWorld ?? world;
            Entity User = createWorld.userRoot.entity;
            Entity par = User.parent.target;
            var (cube, mesh) = Helpers.MeshHelper.AddMesh<BoxMesh>(par, "Cube");
            var headPos = createWorld.userRoot.Headpos;
            var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0,-1,-5));
            cube.setGlobalTrans(move*headPos);
            var col = cube.attachComponent<BoxCollider>();
            col.mass.value = 10;
            col.NoneStaticBody.value = true;
            cube.attachComponent<Grabbable>();
        }

        public override void OnAttach()
        {
            var e = entity.addChild("TaskBar");
            root.target = e;
            BasicUnlitShader shader = world.staticAssets.basicUnlitShader;
            CurvedPlaneMesh bmesh = e.attachComponent<CurvedPlaneMesh>();
            bmesh.BottomRadius.value = engine.settingsObject.UISettings.TaskBarCurve;
            bmesh.TopRadius.value = engine.settingsObject.UISettings.TaskBarCurve + 10f;
            bmesh.Height.value = 0.12f;
            bmesh.Width.value = 0.95f;
            MeshInputPlane bmeshcol = e.attachComponent<MeshInputPlane>();
            bmeshcol.mesh.target = bmesh;
            //InputPlane bmeshcol = e.attachComponent<InputPlane>();

            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(90f, -90f, -90f);
            e.position.value = new Vector3f(0, -0.5, -1);
            RMaterial mit = e.attachComponent<RMaterial>();
            var TaskBar = e.addChild("TaskBarUI");
            MeshRender meshRender = TaskBar.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = TaskBar.attachComponent<ImGUICanvas>();
            imGUICanvas.scale.value = bmeshcol.pixelSize.value = new Vector2u(((uint)(7.69 * engine.settingsObject.UISettings.TaskBarCurve)) * 2, 76 * 2);
            imGUICanvas.imputPlane.target = bmeshcol;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            imGUICanvas.noCloseing.value = true;
            imGUICanvas.noBackground.value = true;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            taskbarcanvas.target = imGUICanvas;
            var group = TaskBar.attachComponent<ImGUIBeginRow>();
            var buton = TaskBar.attachComponent<ImGUIImageButton>();
            var items = TaskBar.attachComponent<ImGUIBeginChild>();
            var timeText = TaskBar.attachComponent<ImGUIText>();
            dateTextDriver.target = timeText.text;
            buton.big.value = Colorf.Black;
            var rhutext = TaskBar.attachComponent<RhubarbTextue2D>();
            buton.texture.target = rhutext;
            imGUICanvas.element.target = group;
            buton.action.Target = startMenuClick;
            buton.size.value = new Vector2f(60 * 2);
            group.children.Add().target = buton;
            group.children.Add().target = items;
            group.children.Add().target = timeText;
            buildStartMenu(e.addChild("Start Menu"));
        }

        private DateTime opened = DateTime.UtcNow;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (dateTextDriver.Linked)
            {
                var now = DateTime.Now;
                dateTextDriver.Drivevalue = $"{((now.Hour>12)? $"pm {now.Hour-12}":((now.Hour == 0)? "pm 12" : $"am {now.Hour}"))}:{((now.Minute < 10) ? $"0{now.Minute}" : now.Minute)}\n";
                dateTextDriver.Drivevalue += $"{now.Month}/{((now.Day < 10) ? $"0{now.Day}" : now.Day)}/{now.Year}\n";
            }
            if (DateTime.UtcNow <= opened + new TimeSpan(0, 0, 1)) return;
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

        public TaskBarManager(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public TaskBarManager()
        {
        }
    }
}
