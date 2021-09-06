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
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using g3;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Input;
using System.Numerics;
using RhubarbEngine.Components.Physics;
using ImGuiNET;

namespace RhubarbEngine.Components.PrivateSpace
{
    public class ContextMenu : UIWidget
    {
        public Sync<Creality> side;

        public SyncRef<Entity> renderEntity;

        public Sync<bool> open;

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            base.ImguiRender(imGuiRenderer, canvas);
            var drawlist = ImGui.GetForegroundDrawList();
            drawlist.AddCircleFilled(new Vector2(300f), 60f, ImGui.GetColorU32(ImGuiCol.WindowBg));
        }

        private void focusLost()
        {
            Close();
        }

        public override void OnAttach()
        {
            base.OnAttach();
            var (renderentity,mesh,mit) = MeshHelper.AddMesh<PlaneMesh>(entity, world.staticAssets.overLayedUnlitShader,"RenderEntity",10);
            renderEntity.target = renderentity;
            mesh.Width.value = 0.5f;
            mesh.Height.value = 0.5f;
            InputPlane col = renderentity.attachComponent<InputPlane>();
            col.onFocusLost.Target = focusLost;
            col.pixelSize.value = new Vector2u(600 * 2);
            col.size.value = new Vector2f(0.25f);
            ImGUICanvas imGUICanvas = renderentity.attachComponent<ImGUICanvas>();
            imGUICanvas.noBackground.value = true;
            imGUICanvas.imputPlane.target = col;
            imGUICanvas.scale.value = new Vector2u(600);
            imGUICanvas.element.target = this;
            imGUICanvas.backGroundColor.value = Colorf.TransparentWhite;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;


            var pos = mit.getField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
            pos.field.value = 0.4f;

            Close();
        }


        public override void buildSyncObjs(bool newRefIds)
        {
            side = new Sync<Creality>(this, newRefIds);
            renderEntity = new SyncRef<Entity>(this, newRefIds);
            open = new Sync<bool>(this, newRefIds, true);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (prossesOpenKey())
            {
                Open();
            }
        }


        public void Open()
        {
            renderEntity.target.enabled.value = true;
            Alline();
            if (open.value) return;
            open.value = true;
        }

        private void Alline()
        {
            Matrix4x4 trans = Matrix4x4.CreateScale(1);
            if(engine.outputType == VirtualReality.OutputType.Screen)
            {
                if (side.value == Creality.Left) return;
                trans = ( Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 1,0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric())) * world.userRoot.Head.target.globalTrans();
            }
            else
            {
                switch (side.value)
                {
                    case Creality.Left:
                        trans = (Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric())) * world.userRoot.LeftHand.target.globalTrans();
                        break;
                    case Creality.Right:
                        trans = (Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric())) * world.userRoot.RightHand.target.globalTrans();
                        break;
                    default:
                        break;
                }
            }
            renderEntity.target.setGlobalTrans(trans);
        }

        public void Close()
        {
            if (!open.value) return;
            renderEntity.target.enabled.value = false;
            open.value = false;
        }
        private bool prossesOpenKey()
        {
            if(engine.outputType == VirtualReality.OutputType.Screen)
            {
                if (side.value == Creality.Left) return false;
                return input.mainWindows.GetKeyDown(Key.F);
            }
            return input.MenuPress(side.value);
        }

        public ContextMenu(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ContextMenu()
        {
        }
    }
}
