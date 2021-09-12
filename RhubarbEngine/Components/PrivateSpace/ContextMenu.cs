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
using RNumerics;
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

		private void FocusLost()
		{
			Close();
		}

		public override void OnAttach()
		{
			base.OnAttach();
			var (renderentity, mesh, mit) = MeshHelper.AddMesh<PlaneMesh>(entity, world.staticAssets.overLayedUnlitShader, "RenderEntity", 10);
			renderEntity.Target = renderentity;
			mesh.Width.Value = 0.5f;
			mesh.Height.Value = 0.5f;
			var col = renderentity.AttachComponent<InputPlane>();
			col.onFocusLost.Target = FocusLost;
			col.pixelSize.Value = new Vector2u(600 * 2);
			col.size.Value = new Vector2f(0.25f);
			var imGUICanvas = renderentity.AttachComponent<ImGUICanvas>();
			imGUICanvas.noBackground.Value = true;
			imGUICanvas.imputPlane.Target = col;
			imGUICanvas.scale.Value = new Vector2u(600);
			imGUICanvas.element.Target = this;
			imGUICanvas.backGroundColor.Value = Colorf.TransparentWhite;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;


			var pos = mit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			pos.field.Value = 0.4f;

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
			if (ProssesOpenKey())
			{
				Open();
			}
		}


		public void Open()
		{
			renderEntity.Target.enabled.Value = true;
			Alline();
			if (open.Value)
            {
                return;
            }

            open.Value = true;
		}

		private void Alline()
		{
			var trans = Matrix4x4.CreateScale(1);
			if (engine.outputType == VirtualReality.OutputType.Screen)
			{
				if (side.Value == Creality.Left)
                {
                    return;
                }

                trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 1, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * world.UserRoot.Head.Target.GlobalTrans();
			}
			else
			{
				switch (side.Value)
				{
					case Creality.Left:
						trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * world.UserRoot.LeftHand.Target.GlobalTrans();
						break;
					case Creality.Right:
						trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * world.UserRoot.RightHand.Target.GlobalTrans();
						break;
					default:
						break;
				}
			}
			renderEntity.Target.SetGlobalTrans(trans);
		}

		public void Close()
		{
			if (!open.Value)
            {
                return;
            }

            renderEntity.Target.enabled.Value = false;
			open.Value = false;
		}
		private bool ProssesOpenKey()
		{
			if (engine.outputType == VirtualReality.OutputType.Screen)
			{
                return side.Value != Creality.Left && input.mainWindows.GetKeyDown(Key.F);
            }
            return input.MenuPress(side.Value);
		}

		public ContextMenu(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ContextMenu()
		{
		}
	}
}
