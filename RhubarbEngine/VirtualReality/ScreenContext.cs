using RhubarbEngine.Input.Controllers;

using System;
using System.Numerics;

using Valve.VR;

using Veldrid;
using Veldrid.Sdl2;

namespace RhubarbEngine.VirtualReality
{
	internal class ScreenContext : VRContext
	{
		private readonly VRContextOptions _options;
		private readonly ScreenMirrorTexture _mirrorTexture;
        private readonly string _deviceName = "Screen";
		private Framebuffer _leftEyeFB;
		private Matrix4x4 _projLeft;

        public override string DeviceName
        {
            get
            {
                return _deviceName;
            }
        }

        public override Framebuffer LeftEyeFramebuffer
        {
            get
            {
                return _leftEyeFB;
            }
        }

        public override Framebuffer RightEyeFramebuffer
        {
            get
            {
                return null;
            }
        }

        internal GraphicsDevice GraphicsDevice { get; private set; }

        //throw new NotImplementedException()
        public override IController LeftController
        {
            get
            {
                return null;
            }
        }

        //throw new NotImplementedException()
        public override IController RightController
        {
            get
            {
                return null;
            }
        }

        public override Matrix4x4 Headpos
        {
            get
            {
                return HeadPos;
            }
        }

        private float _horizontalAngle;

		private float _verticalAngle;

		public float VerticalMin = -90f;

		public float VerticalMax = 90f;

        public Matrix4x4 HeadPos
        {
            get
            {
                return Matrix4x4.CreateScale(1.0f) * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(_horizontalAngle, 0, 0) * Quaternion.CreateFromYawPitchRoll(0, _verticalAngle, 0)) * Matrix4x4.CreateTranslation(new Vector3(0f, 1.7f, 0f));
            }
        }

        private readonly Engine _eng;


		public ScreenContext(VRContextOptions options, Engine eng)
		{
			_options = options;
			_eng = eng;
			_mirrorTexture = new ScreenMirrorTexture(this);
		}

		internal static bool IsSupported()
		{
			return true;
		}

		public void ChangeProject(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
		{
			_projLeft = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
		}

		public override void Initialize(GraphicsDevice gd)
		{
			GraphicsDevice = gd;
			_leftEyeFB = _eng.settingsObject.RenderSettings.DesktopRenderSettings.auto
                ? CreateFramebuffer((uint)_eng.windowManager.MainWindow.Width, (uint)_eng.windowManager.MainWindow.Height)
                : CreateFramebuffer((uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.y);
            _eng.windowManager.MainWindow.window.Resized += Window_Resized;
			if (_leftEyeFB == null)
			{
				Logger.Log("Error Loading Frame Buffer", true);
			}
			ChangeProject((float)(Math.PI / 180) * _eng.renderManager.FieldOfView, _eng.renderManager.AspectRatio, _eng.renderManager.nearPlaneDistance, _eng.renderManager.farPlaneDistance);
		}

		private void Window_Resized()
		{
			if (_eng.settingsObject.RenderSettings.DesktopRenderSettings.auto)
			{
				var oldbuf = _leftEyeFB;
				Console.WriteLine(_eng.windowManager.MainWindow.Width.ToString());

				_leftEyeFB = CreateFramebuffer((uint)_eng.windowManager.MainWindow.Width, (uint)_eng.windowManager.MainWindow.Height);
				oldbuf.ColorTargets[0].Target.Dispose();
				oldbuf.DepthTarget?.Target.Dispose();
				oldbuf.Dispose();
				_mirrorTexture.ClearLeftSet();
			}
			else
			{
				var oldbuf = _leftEyeFB;
				_leftEyeFB = CreateFramebuffer((uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.y);
				oldbuf.ColorTargets[0].Target.Dispose();
				oldbuf.DepthTarget?.Target.Dispose();
				oldbuf.Dispose();
				_mirrorTexture.ClearLeftSet();
			}
			ChangeProject((float)(Math.PI / 180) * _eng.renderManager.FieldOfView, _eng.renderManager.AspectRatio, _eng.renderManager.nearPlaneDistance, _eng.renderManager.farPlaneDistance);
		}

		private Framebuffer CreateFramebuffer(uint width, uint height)
		{
			var factory = GraphicsDevice.ResourceFactory;
			var colorTarget = factory.CreateTexture(TextureDescription.Texture2D(
				width, height,
				1, 1,
				PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
				TextureUsage.RenderTarget | TextureUsage.Sampled,
				_options.EyeFramebufferSampleCount));
			var depthTarget = factory.CreateTexture(TextureDescription.Texture2D(
				width, height,
				1, 1,
				PixelFormat.R32_Float,
				TextureUsage.DepthStencil,
				_options.EyeFramebufferSampleCount));
			return factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
		}
		public override (string[] instance, string[] device) GetRequiredVulkanExtensions()
		{
			return (Array.Empty<string>(), Array.Empty<string>());
		}

		public override HmdPoseState WaitForPoses()
		{
			var deviceToAbsolute = HeadPos;
			Matrix4x4.Invert(deviceToAbsolute, out var absoluteToDevice);

			var viewLeft = Matrix4x4.CreateScale(1f) * absoluteToDevice;
			var viewRight = Matrix4x4.CreateScale(1f) * absoluteToDevice;


			Matrix4x4.Invert(viewLeft, out var invViewLeft);
			Matrix4x4.Decompose(invViewLeft, out _, out var leftRotation, out var leftPosition);

			Matrix4x4.Invert(viewRight, out var invViewRight);
			Matrix4x4.Decompose(invViewRight, out _, out var rightRotation, out var rightPosition);

			return new HmdPoseState(
				_projLeft, _projLeft,
				leftPosition, rightPosition,
				leftRotation, rightRotation);
		}

		public override void SubmitFrame()
		{
			if (GraphicsDevice.GetOpenGLInfo(out var glInfo))
			{
				glInfo.FlushAndFinish();
			}
		}

		private Vector2 _mousePressedPos;

		private bool _mousePressed = false;


		public void UpdateInput()
		{
			Vector2 mouseDelta = default;
			if (_eng.inputManager.mainWindows.GetKeyDown(Key.R))
			{
				if (!_mousePressed)
				{
					_mousePressed = true;
					_mousePressedPos = new Vector2(_eng.windowManager.MainWindow.window.Width / 2, _eng.windowManager.MainWindow.window.Height / 2);
					Sdl2Native.SDL_ShowCursor(0);
					Sdl2Native.SDL_SetWindowGrab(_eng.windowManager.MainWindow.window.SdlWindowHandle, true);
				}
				else
				{
					Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.MainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
					Sdl2Native.SDL_SetWindowGrab(_eng.windowManager.MainWindow.window.SdlWindowHandle, false);
					Sdl2Native.SDL_ShowCursor(1);
					_mousePressed = false;
				}
				mouseDelta = Vector2.Zero;
				Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.MainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
			}
			else if (_mousePressed)
			{
				mouseDelta = _mousePressedPos - _eng.inputManager.mainWindows.MousePosition;
				Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.MainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
			}
			if (mouseDelta != default)
			{
				_horizontalAngle += mouseDelta.X * 0.002f;
                _verticalAngle = Math.Clamp(value: _verticalAngle + (mouseDelta.Y * 0.002f), min: VerticalMin / 90, max: VerticalMax / 90);
			}
		}

		public override void RenderMirrorTexture(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
		{
			if (Disposed)
			{
				return;
			}
			_mirrorTexture.Render(cl, fb, source);
		}


		public override void Dispose()
		{

			_leftEyeFB.ColorTargets[0].Target.Dispose();
			_leftEyeFB.DepthTarget?.Target.Dispose();
			_leftEyeFB.Dispose();
		}

	}
}
