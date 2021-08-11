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
        public SyncRef<ImGUICanvas> taskbarcanvas;

        public override void buildSyncObjs(bool newRefIds)
        {
            root = new SyncRef<Entity>(this, newRefIds);
            taskbarcanvas = new SyncRef<ImGUICanvas>(this, newRefIds);
        }

        public void openStartMenu()
        {

        }
        public override void OnAttach()
        {
            var e = entity.addChild("TaskBar");
            root.target = e;
            BasicUnlitShader shader = e.attachComponent<BasicUnlitShader>();
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
            imGUICanvas.scale.value = bmeshcol.pixelSize.value = new Vector2u(((uint)(7.69 * engine.settingsObject.UISettings.TaskBarCurve)), 76);
            imGUICanvas.imputPlane.target = bmeshcol;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            imGUICanvas.noCloseing.value = true;
            imGUICanvas.noBackground.value = true;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            taskbarcanvas.target = imGUICanvas;
            var group = TaskBar.attachComponent<ImGUIBeginGroup>();
            var buton = TaskBar.attachComponent<ImGUIButton>();
            imGUICanvas.element.target = group;
            buton.action.Target = openStartMenu;
            buton.label.value = "start";
            buton.size.value = new Vector2f(60);
            group.children.Add().target = buton;

        }

        private DateTime opened = DateTime.UtcNow;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
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

        public TaskBarManager(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public TaskBarManager()
        {
        }
    }
}
