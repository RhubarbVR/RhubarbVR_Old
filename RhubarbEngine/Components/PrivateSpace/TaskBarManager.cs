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

		private void StartMenuFocusLost()
		{
			if (startMenu.Target == null)
            {
                return;
            }

            if (startMenu.Target.enabled.Value)
			{
				startMenu.Target.enabled.Value = false;
			}
		}

		private void BuildStartMenu(Entity e)
		{
			startMenu.Target = e;
			var shader = World.staticAssets.basicUnlitShader;
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

			imGUICanvas.element.Target = group;
			group.children.Add().Target = createCube;

			createWindow.label.Value = "Create Window";
			createWindow.action.Target = CreateWindow;
			createWindow2.label.Value = "Create Window2";
			createWindow2.action.Target = CreateWindow2;
			group.children.Add().Target = createWindow;
			group.children.Add().Target = createWindow2;
			group.children.Add().Target = createSyer;
			e.enabled.Value = false;
		}

		private void CreateWindow()
		{
			Logger.Log("Create Window");
			var createWorld = World.worldManager.FocusedWorld ?? World;
			var User = createWorld.UserRoot.Entity;
			var par = User.parent.Target;
			var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<HierarchyRoot>(par);
			var headPos = createWorld.UserRoot.Headpos;
			var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
			cube.SetGlobalTrans(move * headPos);
			comp.Initialize(World.worldManager.FocusedWorld.RootEntity);

		}

		private void CreateWindow2()
		{
			Logger.Log("Create Window2");
			var createWorld = World.worldManager.FocusedWorld ?? World;
			var User = createWorld.UserRoot.Entity;
			var par = User.parent.Target;
			var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<EntityObserver>(par);
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
			var shader = World.staticAssets.basicUnlitShader;
			var bmesh = e.AttachComponent<CurvedPlaneMesh>();
			bmesh.BottomRadius.Value = Engine.settingsObject.UISettings.TaskBarCurve;
			bmesh.TopRadius.Value = Engine.settingsObject.UISettings.TaskBarCurve + 10f;
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
			imGUICanvas.scale.Value = bmeshcol.pixelSize.Value = new Vector2u(((uint)(7.69 * Engine.settingsObject.UISettings.TaskBarCurve)) * 2, 76 * 2);
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
				dateTextDriver.Drivevalue += $"FPS {Engine.platformInfo.AvrageFrameRate}";
			}
			if (DateTime.UtcNow <= _opened + new TimeSpan(0, 0, 1))
            {
                return;
            }

            if (((Input.mainWindows.GetKey(Veldrid.Key.ControlLeft) || Input.mainWindows.GetKey(Veldrid.Key.ControlLeft)) && Input.mainWindows.GetKey(Veldrid.Key.Space)) || Input.mainWindows.GetKeyDown(Veldrid.Key.Escape))
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
