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
using RNumerics;
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
using RhubarbEngine.Components.Physics;
using System.Diagnostics;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class TaskBarManager : Component
	{
		public SyncRef<Entity> root;
		public SyncRef<Entity> startMenu;
		public SyncRef<ImGUICanvas> taskbarcanvas;
		public SyncRef<ImGUICanvas> startcanvas;
		public Driver<string> dateTextDriver;

		public override void BuildSyncObjs(bool newRefIds)
		{
			root = new SyncRef<Entity>(this, newRefIds);
			startMenu = new SyncRef<Entity>(this, newRefIds);
			taskbarcanvas = new SyncRef<ImGUICanvas>(this, newRefIds);
			startcanvas = new SyncRef<ImGUICanvas>(this, newRefIds);
			dateTextDriver = new Driver<string>(this, newRefIds);

		}

		public void StartMenuClick()
		{
			if (startMenu.Target == null)
            {
                return;
            }

            startMenu.Target.enabled.Value = !startMenu.Target.enabled.Value;
		}

        private void SpawnMirror()
        {
            Logger.Log("Create Mirror");
            var createWorld = World.worldManager.FocusedWorld ?? World;
            var User = createWorld.UserRoot.Entity;
            var par = User.parent.Target;
            var cube = par.AddChild("Mirror");
            var headPos = createWorld.UserRoot.Headpos;
            var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, -1, -5));
            cube.SetGlobalTrans(move * headPos);
            var entity = cube;
            var mesh = entity.AttachComponent<PlaneMesh>();
            var UIRender = entity.AddChild("UIRender");
            entity.AttachComponent<Grabbable>();
            entity.AttachComponent<BoxCollider>();
            var (_, _, _, mesh2) = Helpers.MeshHelper.AddMeshWithMeshRender<BoxMesh>(entity, World.staticAssets.BasicUnlitShader, "UIBackGround", 2147483646);
            var cam = entity.AttachComponent<Camera2D>();
            var mit = entity.AttachComponent<RMaterial>();
            var meshRender = UIRender.AttachComponent<MeshRender>();
            mit.Shader.Target = World.staticAssets.BasicUnlitShader;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = mesh;
            cam.excludedsRenderObjects.Add().Target = meshRender;
            cam.excludedsRenderObjects.Add().Target = mesh2;
            field.field.Target = cam;
        }


        private void BuildStartMenu(Entity e)
		{
            _timeout.Start();
            startMenu.Target = e;
			var shader = World.staticAssets.BasicUnlitShader;
			var bmesh = e.AttachComponent<PlaneMesh>();
			bmesh.Height.Value = 0.30f;
			bmesh.Width.Value = 0.30f;
			var bmeshcol = e.AttachComponent<InputPlane>();
			//bmeshcol.onFocusLost.Target = startMenuFocusLost;
			bmeshcol.size.Value = new Vector2f(bmesh.Width.Value, bmesh.Height.Value) / 2;
			//InputPlane bmeshcol = e.attachComponent<InputPlane>();
			var mit = e.AttachComponent<RMaterial>();
			var meshRender = e.AttachComponent<MeshRender>();
			meshRender.RenderOrderOffset.Value = 20;
			var imGUICanvas = e.AttachComponent<ImGUICanvas>();
			e.position.Value = new Vector3f(-0.5, 0.05, 0.25);
			imGUICanvas.scale.Value = bmeshcol.pixelSize.Value = new Vector2u(150, 150);
			imGUICanvas.imputPlane.Target = bmeshcol;
			mit.Shader.Target = shader;
			meshRender.Materials.Add().Target = mit;
			meshRender.Mesh.Target = bmesh;
			imGUICanvas.noCloseing.Value = true;
			imGUICanvas.noBackground.Value = true;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;
			startcanvas.Target = imGUICanvas;
			var group = e.AttachComponent<ImGUIBeginGroup>();
			var createWindow = e.AttachComponent<ImGUIButton>();
			var createWindow2 = e.AttachComponent<ImGUIButton>();
			var createCube = e.AttachComponent<ImGUIButton>();
			createCube.label.Value = "Create Cube";
			createCube.action.Target = CreateCube;

			var createSyer = e.AttachComponent<ImGUIButton>();
			createSyer.label.Value = "Create Sphere";
			createSyer.action.Target = CreateSphere;


            var sessionButton = e.AttachComponent<ImGUIButton>();
            sessionButton.label.Value = "Join main Session";
            sessionButton.action.Target = JoinMainSession;

            //var MirrorButton = e.AttachComponent<ImGUIButton>();
            //MirrorButton.label.Value = "Mirror";
            //MirrorButton.action.Target = SpawnMirror;

            imGUICanvas.element.Target = group;
			group.children.Add().Target = createCube;

			createWindow.label.Value = "Hierarchy Window";
			createWindow.action.Target = HierarchyWindow;
			createWindow2.label.Value = "Properties Window";
			createWindow2.action.Target = PropertiesWindow;
			group.children.Add().Target = createWindow;
			group.children.Add().Target = createWindow2;
			group.children.Add().Target = createSyer;
            group.children.Add().Target = sessionButton;
            //group.children.Add().Target = MirrorButton;
            e.enabled.Value = false;
		}

        private readonly Stopwatch _timeout = new();
        private void JoinMainSession()
        {
            if(_timeout.ElapsedMilliseconds < 2000)
            {
                return;
            }
            Engine.WorldManager.CreateNewWorld("Main Session", true, 16, "!NVkSFpkqKmJBtNDhkr:rhubarbvr.net");
            _timeout.Restart();
        }

        private void HierarchyWindow()
		{
			Logger.Log("Create Hierarchy Window");
			var createWorld = World.worldManager.FocusedWorld ?? World;
			var User = createWorld.UserRoot.Entity;
			var par = User.parent.Target;
			var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<HierarchyRoot>(par);
			var headPos = createWorld.UserRoot.Headpos;
			var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
			cube.SetGlobalTrans(move * headPos);
			comp.Initialize(World.worldManager.FocusedWorld.RootEntity);

		}

		private void PropertiesWindow()
		{
			Logger.Log("Create Properties Window");
			var createWorld = World.worldManager.FocusedWorld ?? World;
			var User = createWorld.UserRoot.Entity;
			var par = User.parent.Target;
			var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<EntityProperties>(par);
			var headPos = createWorld.UserRoot.Headpos;
			var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
			cube.SetGlobalTrans(move * headPos);
			comp.target.Target = World.worldManager.FocusedWorld.RootEntity;

		}

		private void CreateCube()
		{
			Logger.Log("Create Cube");
			var createWorld = World.worldManager.FocusedWorld ?? World;
			var User = createWorld.UserRoot.Entity;
			var par = User.parent.Target;
			var (cube, _) = Helpers.MeshHelper.AddMesh<BoxMesh>(par, "Cube");
			var headPos = createWorld.UserRoot.Headpos;
			var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, -1, -5));
			cube.SetGlobalTrans(move * headPos);
			var col = cube.AttachComponent<BoxCollider>();
			col.mass.Value = 1;
			col.NoneStaticBody.Value = true;
			cube.AttachComponent<Grabbable>();
		}
		private void CreateSphere()
		{
			Logger.Log("Create Sphere");
			var createWorld = World.worldManager.FocusedWorld ?? World;
            var User = createWorld.UserRoot.Entity;
            var par = User.parent.Target;
			var (cube, _) = Helpers.MeshHelper.AddMesh<SphereMesh>(par, "Sphere");
			var headPos = createWorld.UserRoot.Headpos;
			var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, -1, -5));
			cube.SetGlobalTrans(move * headPos);
			var col = cube.AttachComponent<SphereCollider>();
			col.mass.Value = 1;
			col.NoneStaticBody.Value = true;
			cube.AttachComponent<Grabbable>();
		}
		public override void OnAttach()
		{
			var e = Entity.AddChild("TaskBar");
			root.Target = e;
			var shader = World.staticAssets.BasicUnlitShader;
			var bmesh = e.AttachComponent<CurvedPlaneMesh>();
			bmesh.BottomRadius.Value = Engine.SettingsObject.UISettings.TaskBarCurve;
			bmesh.TopRadius.Value = Engine.SettingsObject.UISettings.TaskBarCurve + 10f;
			bmesh.Height.Value = 0.12f;
			bmesh.Width.Value = 0.95f;
			var bmeshcol = e.AttachComponent<MeshInputPlane>();
			bmeshcol.mesh.Target = bmesh;
			//InputPlane bmeshcol = e.attachComponent<InputPlane>();

			//e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
			e.rotation.Value = Quaternionf.CreateFromEuler(90f, -90f, -90f);
			e.position.Value = new Vector3f(0, -0.5, -1);
			var mit = e.AttachComponent<RMaterial>();
			var TaskBar = e.AddChild("TaskBarUI");
			var meshRender = TaskBar.AttachComponent<MeshRender>();
			meshRender.RenderOrderOffset.Value = 20;
			var imGUICanvas = TaskBar.AttachComponent<ImGUICanvas>();
			imGUICanvas.scale.Value = bmeshcol.pixelSize.Value = new Vector2u(((uint)(7.69 * Engine.SettingsObject.UISettings.TaskBarCurve)) * 2, 76 * 2);
			imGUICanvas.imputPlane.Target = bmeshcol;
			mit.Shader.Target = shader;
			meshRender.Materials.Add().Target = mit;
			meshRender.Mesh.Target = bmesh;
			imGUICanvas.noCloseing.Value = true;
			imGUICanvas.noBackground.Value = true;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;
			taskbarcanvas.Target = imGUICanvas;
			var group = TaskBar.AttachComponent<ImGUIBeginRow>();
			var buton = TaskBar.AttachComponent<ImGUIImageButton>();
			var items = TaskBar.AttachComponent<ImGUIBeginChild>();
			var timeText = TaskBar.AttachComponent<ImGUIText>();
			dateTextDriver.Target = timeText.text;
			buton.big.Value = Colorf.Black;
			var rhutext = TaskBar.AttachComponent<RhubarbTextue2D>();
			buton.texture.Target = rhutext;
			imGUICanvas.element.Target = group;
			buton.action.Target = StartMenuClick;
			buton.size.Value = new Vector2f(60 * 2);
			group.children.Add().Target = buton;
			group.children.Add().Target = items;
			group.children.Add().Target = timeText;
			BuildStartMenu(e.AddChild("Start Menu"));
		}

		private DateTime _opened = DateTime.UtcNow;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (dateTextDriver.Linked)
			{
				var now = DateTime.Now;
				dateTextDriver.Drivevalue = $"{((now.Hour > 12) ? $"pm {now.Hour - 12}" : ((now.Hour == 0) ? "pm 12" : $"am {now.Hour}"))}:{((now.Minute < 10) ? $"0{now.Minute}" : now.Minute)}\n";
				dateTextDriver.Drivevalue += $"{now.Month}/{((now.Day < 10) ? $"0{now.Day}" : now.Day)}/{now.Year}\n";
				dateTextDriver.Drivevalue += $"FPS {Engine.PlatformInfo.AvrageFrameRate}\n";
                dateTextDriver.Drivevalue += $"DisplayName {Engine.NetApiManager.DisplayName}";
            }
            if (DateTime.UtcNow <= _opened + new TimeSpan(0, 0, 0, 0, 250))
            {
                return;
            }

            if (((Input.MainWindows.GetKey(Veldrid.Key.ControlLeft) || Input.MainWindows.GetKey(Veldrid.Key.ControlLeft)) && Input.MainWindows.GetKey(Veldrid.Key.Space)) || Input.MainWindows.GetKeyDown(Veldrid.Key.Escape))
			{
				Entity.enabled.Value = !Entity.enabled.Value;
				_opened = DateTime.UtcNow;
			}
			if (Input.MenuPress(RhubarbEngine.Input.Creality.None))
			{
                Entity.enabled.Value = !Entity.enabled.Value;
                _opened = DateTime.UtcNow;
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
