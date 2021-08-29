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
        private GraphicsDevice _gd;
        private string _deviceName;
        private Framebuffer _leftEyeFB;
        private Framebuffer _rightEyeFB;
        private Matrix4x4 _projLeft;
        private Matrix4x4 _projRight;
        private Matrix4x4 _headToEyeLeft;
        private Matrix4x4 _headToEyeRight;
        private TrackedDevicePose_t[] _devicePoses = new TrackedDevicePose_t[1];

        public override string DeviceName => _deviceName;

        public override Framebuffer LeftEyeFramebuffer => _leftEyeFB;

        public override Framebuffer RightEyeFramebuffer => _rightEyeFB;

        internal GraphicsDevice GraphicsDevice => _gd;

        //throw new NotImplementedException()
        public override IController leftController => null;

        //throw new NotImplementedException()
        public override IController RightController => null;

        public override Matrix4x4 Headpos => headPos;

        private float HorizontalAngle;

        private float VerticalAngle;

        public float VerticalMin = -90f;

        public float VerticalMax = 90f;

        public Matrix4x4 headPos => Matrix4x4.CreateScale(1.0f) * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(HorizontalAngle, 0,0) * Quaternion.CreateFromYawPitchRoll(0, VerticalAngle, 0)) * Matrix4x4.CreateTranslation(new Vector3(0f, 1.7f, 0f));

        private Engine _eng;


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

        public void changeProject(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            _projLeft = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        public override void Initialize(GraphicsDevice gd)
        {
            _gd = gd;
            if (_eng.settingsObject.RenderSettings.DesktopRenderSettings.auto)
            {
                _leftEyeFB = CreateFramebuffer((uint)_eng.windowManager.mainWindow.width, (uint)_eng.windowManager.mainWindow.height);
            }
            else
            {
                _leftEyeFB = CreateFramebuffer((uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.y);
            }
            _eng.windowManager.mainWindow.window.Resized += Window_Resized;
            if (_leftEyeFB == null)
            {
                Logger.Log("Error Loading Frame Buffer", true);
            }
            changeProject((float)(Math.PI / 180) * _eng.renderManager.fieldOfView, _eng.renderManager.aspectRatio, _eng.renderManager.nearPlaneDistance, _eng.renderManager.farPlaneDistance);
        }

        private void Window_Resized()
        {
            if (_eng.settingsObject.RenderSettings.DesktopRenderSettings.auto)
            {
                var oldbuf = _leftEyeFB;
                Console.WriteLine(_eng.windowManager.mainWindow.width.ToString());
                
                _leftEyeFB = CreateFramebuffer((uint)_eng.windowManager.mainWindow.width, (uint)_eng.windowManager.mainWindow.height);
                oldbuf.ColorTargets[0].Target.Dispose();
                oldbuf.DepthTarget?.Target.Dispose();
                oldbuf.Dispose();
                _mirrorTexture.clearLeftSet();
            }
            else
            {
                var oldbuf = _leftEyeFB;
                _leftEyeFB = CreateFramebuffer((uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)_eng.settingsObject.RenderSettings.DesktopRenderSettings.y);
                oldbuf.ColorTargets[0].Target.Dispose();
                oldbuf.DepthTarget?.Target.Dispose();
                oldbuf.Dispose();
                _mirrorTexture.clearLeftSet();
            }
            changeProject((float)(Math.PI / 180) * _eng.renderManager.fieldOfView, _eng.renderManager.aspectRatio, _eng.renderManager.nearPlaneDistance, _eng.renderManager.farPlaneDistance);
        }

        private Framebuffer CreateFramebuffer(uint width, uint height)
        {
            ResourceFactory factory = _gd.ResourceFactory;
            Texture colorTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                _options.EyeFramebufferSampleCount));
            Texture depthTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil,
                _options.EyeFramebufferSampleCount));
            return factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
        }
        public override (string[] instance, string[] device) GetRequiredVulkanExtensions()
        {
            return (new string[] { }, new string[] { });
        }

        public override HmdPoseState WaitForPoses()
        {
            Matrix4x4 deviceToAbsolute = headPos;
            Matrix4x4.Invert(deviceToAbsolute, out Matrix4x4 absoluteToDevice);

            Matrix4x4 viewLeft = Matrix4x4.CreateScale(1f) * absoluteToDevice;
            Matrix4x4 viewRight = Matrix4x4.CreateScale(1f) * absoluteToDevice;


            Matrix4x4.Invert(viewLeft, out Matrix4x4 invViewLeft);
            Matrix4x4.Decompose(invViewLeft, out _, out Quaternion leftRotation, out Vector3 leftPosition);

            Matrix4x4.Invert(viewRight, out Matrix4x4 invViewRight);
            Matrix4x4.Decompose(invViewRight, out _, out Quaternion rightRotation, out Vector3 rightPosition);

            return new HmdPoseState(
                _projLeft, _projRight,
                leftPosition, rightPosition,
                leftRotation, rightRotation);
        }

        public override void SubmitFrame()
        {
            if (_gd.GetOpenGLInfo(out BackendInfoOpenGL glInfo))
            {
                glInfo.FlushAndFinish();
            }
        }

        private Vector2 _mousePressedPos;

        private bool _mousePressed = false;


        public void updateInput()
        {
            Vector2 mouseDelta = default;
            if (_eng.inputManager.mainWindows.GetKeyDown(Key.R))
            {
                if (!_mousePressed)
                {
                    _mousePressed = true;
                    _mousePressedPos = new Vector2(_eng.windowManager.mainWindow.window.Width / 2, _eng.windowManager.mainWindow.window.Height / 2);
                    Sdl2Native.SDL_ShowCursor(0);
                    Sdl2Native.SDL_SetWindowGrab(_eng.windowManager.mainWindow.window.SdlWindowHandle, true);
                }
                else
                {
                    Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                    Sdl2Native.SDL_SetWindowGrab(_eng.windowManager.mainWindow.window.SdlWindowHandle, false);
                    Sdl2Native.SDL_ShowCursor(1);
                    _mousePressed = false;
                }
                mouseDelta = Vector2.Zero;
                Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
            }
            else if (_mousePressed)
            {
                mouseDelta = (_mousePressedPos - _eng.inputManager.mainWindows.MousePosition);
                Sdl2Native.SDL_WarpMouseInWindow(_eng.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
            }
            if (mouseDelta != default)
            {
                HorizontalAngle += (mouseDelta.X * 0.002f);
                VerticalAngle = Math.Clamp(VerticalAngle + (mouseDelta.Y * 0.002f), VerticalMin/90, VerticalMax/90);
            }
        }

        public override void RenderMirrorTexture(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
        {
            if (Disposed)
            {
                return;
            }
            updateInput();
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
