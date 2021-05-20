using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Valve.VR;
using Veldrid;
using Veldrid.Vk;

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
            _leftEyeFB = CreateFramebuffer((uint)_eng.windowManager.mainWindow.width, (uint)_eng.windowManager.mainWindow.height);

            changeProject(_eng.renderManager.fieldOfView, _eng.renderManager.aspectRatio, _eng.renderManager.nearPlaneDistance, _eng.renderManager.farPlaneDistance);
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
            StringBuilder sb = new StringBuilder(1024);
            string[] instance = sb.ToString().Split(' ');
            sb.Clear();
            string[] device = sb.ToString().Split(' ');
            return (instance, device);
        }

        public override HmdPoseState WaitForPoses()
        {
            Matrix4x4 viewLeft = Matrix4x4.CreateScale(1f);
            Matrix4x4 viewRight = Matrix4x4.CreateScale(1f);

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
