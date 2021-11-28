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
using RhubarbEngine.Components.Physics;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using Veldrid;
using ImGuiNET;
namespace RhubarbEngine.Components.Interaction
{
    [Category(new string[] { "Interaction" })]
    public class Window : UIWidget
	{
		public enum DockPos
		{
			Up,
			Down,
			Left,
			Right
		}

		public SyncRef<IUIElement> element;

		public SyncRef<ImGUICanvas> canvas;

		public Sync<Vector2f> size;

		public Sync<uint> pixelDensity;

		public Driver<string> labelDriver;
		public Driver<float> meshWidth;
		public Driver<float> meshHeight;
		public Driver<Vector2f> colDriver;
		public Driver<Vector3d> BackGround;

		public Driver<Vector2u> colPixelsizeDriver;
		public Driver<Vector2u> canvasPixelsizeDriver;
		public Driver<bool> renderEnableDriver;

		public Sync<DockPos> ChildDocPos;

		public SyncRef<Window> ChildDock;
		public SyncRef<Window> ParentDock;

		public void Dock(Window window, DockPos pos)
		{
			if (World.grabedWindow == this)
			{
				World.grabedWindow = null;
			}
			window.ChildDock.Target = this;
			ParentDock.Target = this;
			window.ChildDocPos.Value = pos;
			Entity.SetParent(window.Entity, false, true);
			SizeUpdate();
		}

		public void UnDock()
		{
			if (ParentDock.Target == null)
            {
                return;
            }

            Entity.SetParent(ParentDock.Target.Entity.parent.Target, false, true);
			SizeUpdate();
		}

		private bool _clapsed;

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var pos = ImGui.GetCursorPos();
			if ((World.grabedWindow != this) && (World.grabedWindow != null) && (ChildDock.Target == null))
			{
				var size = ImGui.GetWindowSize() / 2;
				ImGui.SetCursorPos(size + new Vector2(-20, 0));
				if (ImGui.ArrowButton("left" + ReferenceID.ToString(), ImGuiDir.Left))
				{
					World.grabedWindow.Dock(this, DockPos.Left);
				}
				ImGui.SetCursorPos(size + new Vector2(20, 0));
				if (ImGui.ArrowButton("right" + ReferenceID.ToString(), ImGuiDir.Right))
				{
					World.grabedWindow.Dock(this, DockPos.Right);
				}
				ImGui.SetCursorPos(size + new Vector2(0, -20));
				if (ImGui.ArrowButton("up" + ReferenceID.ToString(), ImGuiDir.Up))
				{
					World.grabedWindow.Dock(this, DockPos.Up);
				}
				ImGui.SetCursorPos(size + new Vector2(0, 20));
				if (ImGui.ArrowButton("down" + ReferenceID.ToString(), ImGuiDir.Down))
				{
					World.grabedWindow.Dock(this, DockPos.Down);
				}
				ImGui.SetCursorPos(pos);
			}
			if (ChildDock.Target == null)
			{
				element.Target?.ImguiRender(imGuiRenderer, canvas);
			}
			else
			{
				var titleBarHeight = ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.Y * 2.0f);
				var framePad = ImGui.GetStyle().FramePadding.Y * 2.0f * 8f;
				var size = ImGui.GetWindowSize();
				var parentsize = new Vector2();
				var flip = false;
				switch (ChildDocPos.Value)
				{
					case DockPos.Up:
						parentsize = new Vector2(size.X, _clapsed ? (size.Y - (titleBarHeight + framePad)) : ((size.Y - (titleBarHeight + framePad)) / 2));
						break;
					case DockPos.Down:
						parentsize = new Vector2(size.X, _clapsed ? (size.Y - (titleBarHeight + framePad)) : ((size.Y - (titleBarHeight + framePad)) / 2));
						flip = true;
						break;
					case DockPos.Left:
						parentsize = new Vector2(_clapsed ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
						flip = true;
						break;
					case DockPos.Right:
						parentsize = new Vector2(_clapsed ? size.X : (size.X / 2), size.Y - (titleBarHeight * 2));
						break;
					default:
						break;
				}
				if (flip)
				{
					ImGui.SetCursorPos(pos);
					if (ImGui.BeginChildFrame((uint)ReferenceID.id, parentsize))
					{
						element.Target?.ImguiRender(imGuiRenderer, canvas);
						ImGui.EndChildFrame();
					}
					var e = true;
					if ((int)ChildDocPos.Value <= 2)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y));
					}
					else
					{
						if (_clapsed)
						{
							ImGui.SetCursorPos(pos + new Vector2(parentsize.X / 2, framePad * 2f));
						}
						else
						{
							ImGui.SetCursorPos(pos + new Vector2(parentsize.X, framePad * 2f));
						}
						ImGui.SetNextItemWidth(size.X / 2);
					}
					if (ImGui.CollapsingHeader(ChildDock.Target.Entity.name.Value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
					{
						_clapsed = false;
						if (ImGui.BeginChildFrame((uint)ReferenceID.id + 1, parentsize))
						{
							ChildDock.Target?.ImguiRender(imGuiRenderer, canvas);
							ImGui.EndChildFrame();
						}
					}
					else
					{
						_clapsed = true;
					}
					if (!e)
					{
						ChildDock.Target.Close();
					}
				}
				else
				{
					if ((int)ChildDocPos.Value < 2)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, parentsize.Y + titleBarHeight));
					}
					else
					{
						ImGui.SetCursorPos(pos + new Vector2(parentsize.X, titleBarHeight));
					}
					if (_clapsed)
					{
						ImGui.SetCursorPos(pos + new Vector2(0f, titleBarHeight));
					}
					if (ImGui.BeginChildFrame((uint)ReferenceID.id, parentsize))
					{
						element.Target?.ImguiRender(imGuiRenderer, canvas);
						ImGui.EndChildFrame();
					}
					ImGui.SetCursorPos(pos);
					var e = true;
					ImGui.SetNextItemWidth(parentsize.X);
					if (ImGui.CollapsingHeader(ChildDock.Target.Entity.name.Value ?? "Null", ref e, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowItemOverlap))
					{
						ImGui.SetNextItemWidth(parentsize.X);
						_clapsed = false;
						if (ImGui.BeginChildFrame((uint)ReferenceID.id + 1, parentsize))
						{
							ChildDock.Target?.ImguiRender(imGuiRenderer, canvas);
							ImGui.EndChildFrame();
						}
					}
					else
					{
						_clapsed = true;
					}
					if (!e)
					{
						ChildDock.Target.Close();
					}
				}

			}

		}

		private void SizeUpdate()
		{
			if (renderEnableDriver.Linked)
			{
				renderEnableDriver.Drivevalue = ParentDock.Target == null;
			}
			if (BackGround.Linked)
			{
				BackGround.Drivevalue = new Vector3f(size.Value.x / 2, 0.001f, size.Value.y / 2);
			}
			if (labelDriver.Linked)
			{
				labelDriver.Drivevalue = Entity.name.Value;
			}
			if (meshWidth.Linked)
			{
				meshWidth.Drivevalue = size.Value.x;
			}
			if (meshHeight.Linked)
			{
				meshHeight.Drivevalue = size.Value.y;
			}
			if (colDriver.Linked)
			{
				colDriver.Drivevalue = size.Value / 2;
			}
			var pixsize = new Vector2u((uint)(size.Value.x * pixelDensity.Value), (uint)(size.Value.y * pixelDensity.Value));
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
			Entity.AttachComponent<Grabbable>();
            var (_, mesh, _) = Helpers.MeshHelper.AddMesh<BoxMesh>(Entity, World.staticAssets.BasicUnlitShader, "UIBackGround", 2147483646);
			BackGround.SetDriveTarget(mesh.Extent);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			Entity.OnDrop += Entity_onDrop;
		}

		private void Entity_onDrop(GrabbableHolder obj, bool laser)
		{
			World.grabedWindow = null;
		}

		private void OnGrabHeader(GrabbableHolder grabbableHolder)
		{
			if (ParentDock.Target != null)
			{
				UnDock();
			}
			foreach (var grab in Entity.GetAllComponents<Grabbable>())
			{
				grab.RemoteGrab(grabbableHolder);
			}
			World.grabedWindow = this;
		}

		public override void OnAttach()
		{
			base.OnAttach();
			var mesh = Entity.AttachComponent<PlaneMesh>();
			meshWidth.SetDriveTarget(mesh.Width);
			meshHeight.SetDriveTarget(mesh.Height);
			var UIRender = Entity.AddChild("UIRender");
			renderEnableDriver.SetDriveTarget(UIRender.enabled);
			AttachBackGround();
			var col = UIRender.AttachComponent<InputPlane>();
			colDriver.SetDriveTarget(col.size);
			colPixelsizeDriver.SetDriveTarget(col.pixelSize);
			var mit = Entity.AttachComponent<RMaterial>();
			var meshRender = UIRender.AttachComponent<MeshRender>();
			UIRender.position.Value = new Vector3f(0f, -0.01f, 0f);
			var imGUICanvas = UIRender.AttachComponent<ImGUICanvas>();
			imGUICanvas.onClose.Target = Close;
			imGUICanvas.imputPlane.Target = col;
			imGUICanvas.scale.Value = new Vector2u(300);
			imGUICanvas.onHeaderGrab.Target = OnGrabHeader;
			labelDriver.SetDriveTarget(imGUICanvas.name);
			imGUICanvas.element.Target = this;
			canvasPixelsizeDriver.SetDriveTarget(imGUICanvas.scale);
			mit.Shader.Target = World.staticAssets.BasicUnlitShader;
			canvas.Target = imGUICanvas;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = imGUICanvas;
			meshRender.Materials.Add().Target = mit;
			meshRender.Mesh.Target = mesh;
			SizeUpdate();
		}

		public void Close()
		{
			Entity.Destroy();
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			element = new SyncRef<IUIElement>(this, newRefIds);
			canvas = new SyncRef<ImGUICanvas>(this, newRefIds);
            size = new Sync<Vector2f>(this, newRefIds)
            {
                Value = new Vector2f(1, 1.5)
            };
            size.Changed += Size_Changed;
            pixelDensity = new Sync<uint>(this, newRefIds)
            {
                Value = 450
            };
            meshWidth = new Driver<float>(this, newRefIds);
			meshHeight = new Driver<float>(this, newRefIds);
			colDriver = new Driver<Vector2f>(this, newRefIds);
			colPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
			canvasPixelsizeDriver = new Driver<Vector2u>(this, newRefIds);
			labelDriver = new Driver<string>(this, newRefIds);
			BackGround = new Driver<Vector3d>(this, newRefIds);
			renderEnableDriver = new Driver<bool>(this, newRefIds);
			ChildDocPos = new Sync<DockPos>(this, newRefIds);
			ChildDock = new SyncRef<Window>(this, newRefIds);
			ParentDock = new SyncRef<Window>(this, newRefIds);
		}

		private void Size_Changed(IChangeable obj)
		{
			SizeUpdate();
		}

		public Window(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public Window()
		{
		}
	}
}
