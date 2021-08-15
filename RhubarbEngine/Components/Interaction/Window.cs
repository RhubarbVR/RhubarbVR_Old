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
using Veldrid;
using ImGuiNET;

namespace RhubarbEngine.Components.Interaction
{
    public class Window : UIWidget
    {
        public enum DockPos
        {
            Up,
            Down,
            Left,
            Right
        }

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
        public Driver<bool> renderEnableDriver;

        public Sync<DockPos> ChildDocPos;

        public SyncRef<Window> ChildDock;
        public SyncRef<Window> ParentDock;

        public void Dock(Window window, DockPos pos)
        {
            if (world.grabedWindow == this)
            {
                world.grabedWindow = null;
            }
            window.ChildDock.target = this;
            ParentDock.target = this;
            window.ChildDocPos.value = pos;
            entity.SetParent(window.entity, false, true);
            sizeUpdate();
        }

        public void unDock()
        {
            if (ParentDock.target == null) return;
            entity.SetParent(ParentDock.target.entity.parent.target, false, true);
            sizeUpdate();
        }

        private bool clapsed;

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            var pos = ImGui.GetCursorPos();
            if ((world.grabedWindow != this) && (world.grabedWindow != null) && (ChildDock.target == null))
            {
                var size = ImGui.GetWindowSize() / 2;
                ImGui.SetCursorPos(size + new Vector2(-20, 0));
                if (ImGui.ArrowButton("left" + referenceID.ToString(), ImGuiDir.Left))
                {
                    world.grabedWindow.Dock(this, DockPos.Left);
                }
                ImGui.SetCursorPos(size + new Vector2(20, 0));
                if (ImGui.ArrowButton("right" + referenceID.ToString(), ImGuiDir.Right))
                {
                    world.grabedWindow.Dock(this, DockPos.Right);
                }
                ImGui.SetCursorPos(size + new Vector2(0, -20));
                if (ImGui.ArrowButton("up" + referenceID.ToString(), ImGuiDir.Up))
                {
                    world.grabedWindow.Dock(this, DockPos.Up);
                }
                ImGui.SetCursorPos(size + new Vector2(0, 20));
                if (ImGui.ArrowButton("down" + referenceID.ToString(), ImGuiDir.Down))
                {
                    world.grabedWindow.Dock(this, DockPos.Down);
                }
                ImGui.SetCursorPos(pos);
            }
            if (ChildDock.target == null)
            {
                element.target?.ImguiRender(imGuiRenderer);
            }
            else
            {
                float titleBarHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2.0f;
                float framePad = (ImGui.GetStyle().FramePadding.Y * 2.0f) * 8f;
                var size = ImGui.GetWindowSize();
                var parentsize = new Vector2();
                bool flip = false;
                switch (ChildDocPos.value)
                {
                    case DockPos.Up:
                        parentsize = new Vector2(size.X, (clapsed) ? ((size.Y - (titleBarHeight + framePad))) : ((size.Y - (titleBarHeight + framePad)) / 2));
                        break;
                    case DockPos.Down:
                        parentsize = new Vector2(size.X, (clapsed) ? ((size.Y - (titleBarHeight + framePad))) : ((size.Y - (titleBarHeight + framePad)) / 2));
                        flip = true;
                        break;
                    case DockPos.Left:
                        parentsize = new Vector2((clapsed) ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
                        flip = true;
                        break;
                    case DockPos.Right:
                        parentsize = new Vector2((clapsed) ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
                        break;
                    default:
                        break;
                }
                if (flip)
                {
                    ImGui.SetCursorPos(pos);
                    if (ImGui.BeginChildFrame((uint)referenceID.id, parentsize))
                    {
                        element.target?.ImguiRender(imGuiRenderer);
                        ImGui.EndChildFrame();
                    }
                    bool e = true;
                    if ((int)ChildDocPos.value <= 2)
                    {
                        ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y));
                    }
                    else
                    {
                        if (clapsed)
                        {
                            ImGui.SetCursorPos(pos + new Vector2(parentsize.X / 2, framePad * 2f));
                        }
                        else
                        {
                            ImGui.SetCursorPos(pos + new Vector2(parentsize.X, framePad * 2f));
                        }
                        ImGui.SetNextItemWidth((size.X / 2));
                    }
                    if (ImGui.CollapsingHeader(ChildDock.target.entity.name.value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
                    {
                        clapsed = false;
                        if (ImGui.BeginChildFrame((uint)referenceID.id + 1, parentsize))
                        {
                            ChildDock.target?.ImguiRender(imGuiRenderer);
                            ImGui.EndChildFrame();
                        }
                    }
                    else
                    {
                        clapsed = true;
                    }
                    if (!e)
                    {
                        ChildDock.target.Close();
                    }
                }
                else
                {
                    if ((int)ChildDocPos.value < 2)
                    {
                        ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y + titleBarHeight));
                    }
                    else
                    {
                        ImGui.SetCursorPos(pos + new Vector2(parentsize.X, titleBarHeight));
                    }
                    if (clapsed)
                    {
                        ImGui.SetCursorPos(pos + new Vector2(0f, titleBarHeight));
                    }
                    if (ImGui.BeginChildFrame((uint)referenceID.id, parentsize))
                    {
                        element.target?.ImguiRender(imGuiRenderer);
                        ImGui.EndChildFrame();
                    }
                    ImGui.SetCursorPos(pos);
                    bool e = true;
                    ImGui.SetNextItemWidth(parentsize.X);
                    if (ImGui.CollapsingHeader(ChildDock.target.entity.name.value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
                    {
                        ImGui.SetNextItemWidth(parentsize.X);
                        clapsed = false;
                        if (ImGui.BeginChildFrame((uint)referenceID.id + 1, parentsize))
                        {
                            ChildDock.target?.ImguiRender(imGuiRenderer);
                            ImGui.EndChildFrame();
                        }
                    }
                    else
                    {
                        clapsed = true;
                    }
                    if (!e)
                    {
                        ChildDock.target.Close();
                    }
                }

            }

        }

        private void sizeUpdate()
        {
            if (renderEnableDriver.Linked)
            {
                renderEnableDriver.Drivevalue = ParentDock.target == null;
            }
            if (colBackGround.Linked)
            {
                colBackGround.Drivevalue = new Vector3f(size.value.x / 2, 0.01f, size.value.y / 2);
            }
            if (BackGround.Linked)
            {
                BackGround.Drivevalue = new Vector3f(size.value.x / 2, 0.01f, size.value.y / 2);
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
                colDriver.Drivevalue = size.value / 2;
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
            var (e, mesh) = Helpers.MeshHelper.AddMesh<BoxMesh>(entity, "UIBackGround");
            BackGround.setDriveTarget(mesh.Extent);
            colBackGround.setDriveTarget(col.boxExtents);
        }

        public override void onLoaded()
        {
            base.onLoaded();
            entity.onDrop += Entity_onDrop;
        }

        private void Entity_onDrop(GrabbableHolder obj)
        {
            world.grabedWindow = null;
        }

        private void OnGrabHeader()
        {
            if (ParentDock.target != null)
            {
                unDock();
            }
            foreach (var grab in entity.getAllComponents<Grabbable>())
            {
                grab.RemoteGrab();
            }
            world.grabedWindow = this;
        }

        public override void OnAttach()
        {
            base.OnAttach();
            PlaneMesh mesh = entity.attachComponent<PlaneMesh>();
            meshWidth.setDriveTarget(mesh.Width);
            meshHeight.setDriveTarget(mesh.Height);
            var UIRender = entity.addChild("UIRender");
            renderEnableDriver.setDriveTarget(UIRender.enabled);
            AttachBackGround();
            InputPlane col = UIRender.attachComponent<InputPlane>();
            colDriver.setDriveTarget(col.size);
            colPixelsizeDriver.setDriveTarget(col.pixelSize);
            RMaterial mit = entity.attachComponent<RMaterial>();
            MeshRender meshRender = UIRender.attachComponent<MeshRender>();
            UIRender.position.value = new Vector3f(0f, -0.012f, 0f);
            ImGUICanvas imGUICanvas = UIRender.attachComponent<ImGUICanvas>();
            imGUICanvas.onClose.Target = Close;
            imGUICanvas.imputPlane.target = col;
            imGUICanvas.scale.value = new Vector2u(300);
            imGUICanvas.onHeaderGrab.Target = OnGrabHeader;
            labelDriver.setDriveTarget(imGUICanvas.name);
            imGUICanvas.element.target = this;
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
            renderEnableDriver = new Driver<bool>(this, newRefIds);
            ChildDocPos = new Sync<DockPos>(this, newRefIds);
            ChildDock = new SyncRef<Window>(this, newRefIds);
            ParentDock = new SyncRef<Window>(this, newRefIds);
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
