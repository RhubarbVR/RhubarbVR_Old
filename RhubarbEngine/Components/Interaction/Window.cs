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


namespace RhubarbEngine.Components.Interaction
{
    public class Window : Component
    {
        public SyncRef<UIWidget> element;

        public SyncRef<ImGUICanvas> canvas;

        public Sync<Vector2f> size;

        public Sync<uint> pixelDensity;

        public Driver<string> labelDriver;
        public Driver<float> meshWidth;
        public Driver<float> meshHeight;
        public Driver<Vector2f> colDriver;
        public Driver<Vector3d> BackGround;
        public Driver<Vector3f> colBackGround;

        public Driver<Vector2u> colPixelsizeDriver;
        public Driver<Vector2u> canvasPixelsizeDriver;

        private void sizeUpdate()
        {
            if (colBackGround.Linked)
            {
                colBackGround.Drivevalue = new Vector3f(size.value.x/2, 0.01f, size.value.y/2); 
            }
            if (BackGround.Linked)
            {
                BackGround.Drivevalue = new Vector3f(size.value.x/2, 0.01f, size.value.y/2);
            }
            if (labelDriver.Linked)
            {
                labelDriver.Drivevalue = entity.name.value;
            }
            if (meshWidth.Linked)
            {
                meshWidth.Drivevalue = size.value.x;
            }
            if (meshHeight.Linked)
            {
                meshHeight.Drivevalue = size.value.y;
            }
            if (colDriver.Linked)
            {
                colDriver.Drivevalue = size.value/2;
            }
            Vector2u pixsize = new Vector2u((uint)(size.value.x * pixelDensity.value), (uint)(size.value.y * pixelDensity.value));
            if (colPixelsizeDriver.Linked)
            {
                colPixelsizeDriver.Drivevalue = pixsize;
            }
            if (canvasPixelsizeDriver.Linked)
            {
                canvasPixelsizeDriver.Drivevalue = pixsize;
            }
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
        }

        private void AttachBackGround()
        {
            entity.attachComponent<Grabbable>();
            var col = entity.attachComponent<BoxCollider>();
            var (e,mesh) = Helpers.MeshHelper.AddMesh<BoxMesh>(entity, "UIBackGround");
            BackGround.setDriveTarget(mesh.Extent);
            colBackGround.setDriveTarget(col.boxExtents);
        }

        private void OnGrabHeader()
        {
            foreach (var grab in entity.getAllComponents<Grabbable>())
            {
                grab.RemoteGrab();
            }
        }

        public override void OnAttach()
        {
            base.OnAttach();
            PlaneMesh mesh = entity.attachComponent<PlaneMesh>();
            meshWidth.setDriveTarget(mesh.Width);
            meshHeight.setDriveTarget(mesh.Height);
            var UIRender = entity.addChild("UIRender");
            AttachBackGround();
            InputPlane col = UIRender.attachComponent<InputPlane>();
            colDriver.setDriveTarget(col.size);
            colPixelsizeDriver.setDriveTarget(col.pixelSize);
            RMaterial mit = entity.attachComponent<RMaterial>();
            MeshRender meshRender = UIRender.attachComponent<MeshRender>();
            UIRender.position.value = new Vector3f(0f, -0.011f, 0f);
            ImGUICanvas imGUICanvas = entity.attachComponent<ImGUICanvas>();
            imGUICanvas.onClose.Target = Close;
            imGUICanvas.imputPlane.target = col;
            imGUICanvas.scale.value = new Vector2u(300);
            imGUICanvas.onHeaderGrab.Target = OnGrabHeader;
            labelDriver.setDriveTarget(imGUICanvas.name);
            var group = entity.attachComponent<ImGUIText>();
            imGUICanvas.element.target = group;
            canvasPixelsizeDriver.setDriveTarget(imGUICanvas.scale);
            mit.Shader.target = world.staticAssets.basicUnlitShader;
            canvas.target = imGUICanvas;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = mesh;
            sizeUpdate();
        }

        public void Close()
        {
            entity.Destroy();
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            element = new SyncRef<UIWidget>(this, newRefIds);
            element.Changed += Element_Changed;
            canvas = new SyncRef<ImGUICanvas>(this, newRefIds);
            size = new Sync<Vector2f>(this, newRefIds);
            size.value = new Vector2f(1, 1.5);
            size.Changed += Size_Changed;
            pixelDensity = new Sync<uint>(this, newRefIds);
            pixelDensity.value = 300;
            meshWidth = new Driver<float>(this, newRefIds);
            meshHeight = new Driver<float>(this, newRefIds);
            colDriver = new Driver<Vector2f>(this, newRefIds);
            colPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
            canvasPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
            labelDriver = new Driver<string>(this, newRefIds);
            BackGround = new Driver<Vector3d>(this, newRefIds);
            colBackGround = new Driver<Vector3f>(this, newRefIds);
        }

        private void Element_Changed(IChangeable obj)
        {
            if(canvas.target != null)
            {
                canvas.target.element.target = element.target;
            }
        }

        private void Size_Changed(IChangeable obj)
        {
            sizeUpdate();
        }

        public Window(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public Window()
        {
        }
    }
}
