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

        public SyncRef<InputPlane> collider;


        private void NormalButton(Action onClick,string name)
        {
            ImGui.SetNextItemOpen(false);
            if(ImGui.TreeNodeEx($"{name}##{name}{ReferenceID.id}", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.Bullet))
            {
                onClick?.Invoke();
                ImGui.TreePop();
            }
        }

        private void NormalDropDown(Action onClick, string name)
        {
            if (ImGui.TreeNodeEx($"{name}##{name}{ReferenceID.id}", ImGuiTreeNodeFlags.Framed))
            {
                onClick?.Invoke();
                ImGui.TreePop();
            }
        }

        private void NormalDropDown(Action onClick, string name,string value)
        {
            if(ImGui.TreeNodeEx($"{name}{value}##{name}{ReferenceID.id}", ImGuiTreeNodeFlags.Framed))
            {
                onClick?.Invoke();
                ImGui.TreePop();
            }
        }
        

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
            ImGui.GetStyle().FramePadding = new Vector2(6);
            if (Engine.OutputType == VirtualReality.OutputType.Screen)
            {
                if ((World.worldManager.FocusedWorld?.HeadLaserGrabbableHolder?.CanDestroyAnyGabbed ?? false) || (World.worldManager.FocusedWorld?.LeftLaserGrabbableHolder?.CanDestroyAnyGabbed ?? false) || (World.worldManager.FocusedWorld?.RightLaserGrabbableHolder?.CanDestroyAnyGabbed ?? false))
                {
                    NormalButton(() =>
                    {
                            if (World.worldManager.FocusedWorld.HeadLaserGrabbableHolder != null)
                            {
                                try
                                {
                                    World.worldManager.FocusedWorld.HeadLaserGrabbableHolder.DeleteGrabObjects();
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("Failed To Delete " + e.ToString(), true);
                                }
                            }
                            if (World.worldManager.FocusedWorld.LeftLaserGrabbableHolder != null)
                            {
                                try
                                {
                                    World.worldManager.FocusedWorld.LeftLaserGrabbableHolder.DeleteGrabObjects();
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("Failed To Delete " + e.ToString(), true);
                                }
                            }
                            if (World.worldManager.FocusedWorld.RightLaserGrabbableHolder != null)
                            {
                                try
                                {
                                    World.worldManager.FocusedWorld.RightLaserGrabbableHolder.DeleteGrabObjects();
                                }
                                catch (Exception e)
                                {
                                    Logger.Log("Failed To Delete " + e.ToString(), true);
                                }
                            }
                        Close();
                    }, "Delete");
                }
            }
            else
            {
                if (World.worldManager.FocusedWorld != null)
                {
                    var holder = (side.Value == Creality.Left) ? World.worldManager.FocusedWorld.LeftLaserGrabbableHolder : World.worldManager.FocusedWorld.RightLaserGrabbableHolder;
                    if (holder?.CanDestroyAnyGabbed??false)
                    {
                        NormalButton(() => {
                            try
                            {
                                holder.DeleteGrabObjects();
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Failed To Delete " + e.ToString(), true);
                            }
                            Close();
                        }, "Delete");
                    }

                }
            }
            if (World.worldManager.PersonalSpace?.Taskbar.Target?.IsOpen??false)
            {
                NormalButton(() => {
                    World.worldManager.PersonalSpace?.Taskbar.Target?.Close();
                    Close();
                }, "Close Dash");
            }
            else
            {
                NormalButton(() => {
                    World.worldManager.PersonalSpace?.Taskbar.Target?.Open();
                    Close();
                }, "Open Dash");
            }
            NormalButton(() => Close(), "Close");
        }

        private float _closeCoutDown = 0.5f;

		private void FocusLost()
		{
            _closeCoutDown = 0.5f;
        }

		public override void OnAttach()
		{
			base.OnAttach();
			var (renderentity, mesh, mit) = MeshHelper.AddMesh<PlaneMesh>(Entity, World.staticAssets.OverLayedUnlitShader, "RenderEntity", 10);
			renderEntity.Target = renderentity;
            var ratio = new Vector2f(1, 1);
            var reslution = 300;
			mesh.Width.Value = ratio.x/2;
			mesh.Height.Value = ratio.y/2;
			var col = renderentity.AttachComponent<InputPlane>();
            collider.Target = col;
			col.onFocusLost.Target = FocusLost;
			col.pixelSize.Value = new Vector2u((uint)(ratio.x * reslution), (uint)(ratio.y * reslution));
			col.size.Value = new Vector2f(ratio.x / 4, ratio.x / 4);
			var imGUICanvas = renderentity.AttachComponent<ImGUICanvas>();
			imGUICanvas.noBackground.Value = true;
			imGUICanvas.imputPlane.Target = col;
			imGUICanvas.scale.Value = new Vector2u((uint)(ratio.x * reslution), (uint)(ratio.y * reslution));
			imGUICanvas.element.Target = this;
			imGUICanvas.backGroundColor.Value = Colorf.TransparentWhite;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;


			var pos = mit.GetField<Render.Material.Fields.FloatField>("Zpos", Render.Shader.ShaderType.MainFrag);
			pos.field.Value = 0.4f;

			Close();
		}


		public override void BuildSyncObjs(bool newRefIds)
		{
			side = new Sync<Creality>(this, newRefIds);
			renderEntity = new SyncRef<Entity>(this, newRefIds);
			open = new Sync<bool>(this, newRefIds, true);
            collider = new SyncRef<InputPlane>(this, newRefIds);

        }

        private bool _click;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
            if(collider.Target?.Focused ?? false)
            {
                _closeCoutDown = 0.5f;
            }
            else if (_closeCoutDown > 0)
            {
                _closeCoutDown -= (float)Engine.PlatformInfo.DeltaSeconds;
            }
            if (!(collider.Target?.Focused ?? false) && open.Value)
            {
                if (_closeCoutDown < 0)
                {
                    Close();
                }
            }

            if (_click != ProssesOpenKey())
			{
                if (!_click)
                {
                    if (open.Value)
                    {
                        Close();
                    }
                    else
                    {
                        Open();
                    }
                }
            }
            _click = ProssesOpenKey();
        }


        public void Open()
		{
            _closeCoutDown = 0.5f;
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
			if (Engine.OutputType == VirtualReality.OutputType.Screen)
			{
				if (side.Value == Creality.Left)
                {
                    return;
                }

                trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 1, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * World.UserRoot.Head.Target.GlobalTrans();
			}
			else
			{
				switch (side.Value)
				{
					case Creality.Left:
						trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * World.UserRoot.LeftHand.Target.GlobalTrans();
						break;
					case Creality.Right:
						trans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric()) * World.UserRoot.RightHand.Target.GlobalTrans();
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
            return Engine.OutputType == VirtualReality.OutputType.Screen
                ? (side.Value != Creality.Left && Input.MainWindows.GetMouseButton(MouseButton.Middle))
                : Input.MenuPress(side.Value);
        }

        public ContextMenu(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ContextMenu()
		{
		}
	}
}
