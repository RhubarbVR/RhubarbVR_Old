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
using ImGuiNET;
using RhubarbEngine.World.Asset;
using Veldrid;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.ImGUI
{
	[Category(new string[] { "ImGUI" })]
	public class ImGUICanvas : AssetProvider<RTexture2D>, IRenderObject, IKeyboardStealer
	{
		public Sync<Vector2u> scale;

		public Sync<RenderFrequency> renderFrequency;

		public SyncRef<IUIElement> element;

		public SyncRef<IInputPlane> imputPlane;

		public Sync<string> name;

		public Sync<bool> noCloseing;

		public Sync<bool> noKeyboard;

		public Sync<bool> noBackground;

		public Sync<Colorf> backGroundColor;

		private ImGuiRenderer _igr;

		private CommandList _uIcommandList;

		private Framebuffer _framebuffer;

		private bool _uIloaded = false;

		public SyncDelegate onClose;
		public SyncDelegate onHeaderGrab;
		public SyncDelegate onHeaderClick;

		public override void LoadListObject()
		{
			try
			{
				World.updateLists.renderObject.SafeAdd(this);
			}
			catch { }
		}

		public override void RemoveListObject()
		{
			try
			{
				World.updateLists.renderObject.Remove(this);
			}
			catch { }
		}


        public RenderFrequency RenderFrac
        {
            get
            {
                return renderFrequency.Value;
            }
        }

        public bool Threaded
        {
            get
            {
                return false;
            }
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			scale = new Sync<Vector2u>(this, newRefIds);
			renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
			scale.Value = new Vector2u(600, 600);
			scale.Changed += OnScaleChange;
			element = new SyncRef<IUIElement>(this, newRefIds);
			imputPlane = new SyncRef<IInputPlane>(this, newRefIds);
			name = new Sync<string>(this, newRefIds);
			noCloseing = new Sync<bool>(this, newRefIds);
			noBackground = new Sync<bool>(this, newRefIds);
			noKeyboard = new Sync<bool>(this, newRefIds);
			onClose = new SyncDelegate(this, newRefIds);
			onHeaderClick = new SyncDelegate(this, newRefIds);
			onHeaderGrab = new SyncDelegate(this, newRefIds);
            backGroundColor = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.Black
            };
        }

		private void OnScaleChange(IChangeable val)
		{
			if (_uIcommandList == null)
            {
                return;
            }

            if ((_framebuffer != null) && (_igr != null) && _uIloaded)
			{
				_uIloaded = false;
				Load(null);
				_framebuffer.Dispose();
				_igr.Dispose();
				LoadUI();
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			_uIcommandList = Engine.RenderManager.Gd.ResourceFactory.CreateCommandList();
			LoadUI();
		}

		private Framebuffer CreateFramebuffer(uint width, uint height)
		{
			var factory = Engine.RenderManager.Gd.ResourceFactory;
			var colorTarget = factory.CreateTexture(TextureDescription.Texture2D(
				width, height,
				1, 1,
				PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
				TextureUsage.RenderTarget | TextureUsage.Sampled
				));
			var depthTarget = factory.CreateTexture(TextureDescription.Texture2D(
				width, height,
				1, 1,
				PixelFormat.R32_Float,
				TextureUsage.DepthStencil));
			return factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
		}

		private void LoadUI()
		{
			try
			{
				Logger.Log("Loading ui");
				if (scale.Value.x < 2 || scale.Value.y < 2)
                {
                    throw new Exception("UI too Small");
                }

                _framebuffer = CreateFramebuffer(scale.Value.x, scale.Value.y);
				_igr = new ImGuiRenderer(Engine.RenderManager.Gd, _framebuffer.OutputDescription, (int)scale.Value.x, (int)scale.Value.y, ColorSpaceHandling.Linear);
				var target = _framebuffer.ColorTargets[0].Target;
				var view = Engine.RenderManager.Gd.ResourceFactory.CreateTextureView(target);
				Load(new RTexture2D(view));
				_uIloaded = true;
			}
			catch (Exception e)
			{
				Logger.Log("ImGUI Error When Loading Error" + e.ToString(), true);
			}
		}

		public class FakeInputSnapshot : InputSnapshot
		{
            public IReadOnlyList<KeyEvent> KeyEvents
            {
                get
                {
                    return new List<KeyEvent>();
                }
            }

            public IReadOnlyList<MouseEvent> MouseEvents
            {
                get
                {
                    return new List<MouseEvent>();
                }
            }

            public IReadOnlyList<char> KeyCharPresses
            {
                get
                {
                    return new List<char>();
                }
            }

            public Vector2 MousePosition
            {
                get
                {
                    return Vector2.Zero;
                }
            }

            public float WheelDelta
            {
                get
                {
                    return 0;
                }
            }

            public bool IsMouseDown(MouseButton button)
			{
				return false;
			}
		}

		private void ImGuiUpdate()
		{
			var ui = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize;
			if (noBackground.Value)
			{
				ui |= ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar;
			}
			bool val;
			var e = true;
            val = !noCloseing.Value ? ImGui.Begin(name.Value ?? "Null", ref e, ui) : ImGui.Begin(name.Value ?? "Null", ui);
            if (val)
			{
				ImGui.SetWindowPos(Vector2.Zero);
				ImGui.SetWindowSize(new Vector2(scale.Value.x, scale.Value.y));
				if (element.Target != null)
				{
					element.Target.ImguiRender(_igr, this);
				}
				else
				{
					ImGui.Text("NUll");
				}
				if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
				{
					var titleBarHeight = (((int)ui & (int)ImGuiWindowFlags.NoTitleBar) == 1) ? 0f : ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.Y * 2.0f);
					var pos = ImGui.GetWindowPos();
					if (ImGui.IsMouseHoveringRect(pos, new Vector2(pos.X + ImGui.GetWindowSize().X, pos.Y + titleBarHeight), false))
                    {
                        onHeaderGrab.Target?.Invoke();
                    }
                }
				if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
				{
					var titleBarHeight = (((int)ui & (int)ImGuiWindowFlags.NoTitleBar) == 1) ? 0f : ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.Y * 2.0f);
					var pos = ImGui.GetWindowPos();
					if (ImGui.IsMouseHoveringRect(pos, new Vector2(pos.X + ImGui.GetWindowSize().X, pos.Y + titleBarHeight), false))
                    {
                        onHeaderClick.Target?.Invoke();
                    }
                }
				if (noKeyboard.Value)
                {
                    return;
                }

                if (ImGui.GetIO().WantTextInput)
				{
					Input.Keyboard = this;
					if (imputPlane.Target != null)
					{
						imputPlane.Target.StopMouse = true;
					}
				}
				else
				{
					if (Input.Keyboard == this)
					{
						Input.Keyboard = null;
					}
					if (imputPlane.Target != null)
					{
						imputPlane.Target.StopMouse = false;
					}
				}
				if (imputPlane.Target != null)
				{
                    imputPlane.Target.SetCursor(RhubarbEngine.Input.CursorsEnumCaster.ImGuiMouse(ImGui.GetMouseCursor()));
				}
				ImGui.End();
			}
			if (!e)
			{
				onClose.Target?.Invoke();
			}
		}
		public static FakeInputSnapshot fakeInputSnapshot = new FakeInputSnapshot();

		public void Render()
		{
			if (!loaded)
            {
                return;
            }

            try
			{
				var inputSnapshot = imputPlane.Target == null ? fakeInputSnapshot : (InputSnapshot)imputPlane.Target;
                _igr.Update((float)Engine.PlatformInfo.deltaSeconds, inputSnapshot);
				ImGuiUpdate();
				_uIcommandList.Begin();
				_uIcommandList.SetFramebuffer(_framebuffer);
				_uIcommandList.ClearColorTarget(0, new RgbaFloat((Vector4)backGroundColor.Value.ToRGBA()));
				_igr.Render(Engine.RenderManager.Gd, _uIcommandList);
				_uIcommandList.End();
				Engine.RenderManager.Gd.SubmitCommands(_uIcommandList);
			}
			catch (Exception e)
			{
				Logger.Log("Error Rendering" + e.ToString(), true);
			}
		}

		public ImGUICanvas(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUICanvas()
		{
		}

	}
}
