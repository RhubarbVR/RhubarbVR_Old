using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Render;


namespace RhubarbEngine.Managers
{
    public class RenderManager : IManager
    {
        private Engine engine;

        private Vector3 _userPosition => engine.worldManager.focusedWorld.playerPosition.ToSystemNumrics();

        private RenderQueue mainQueue;

        public VRContext vrContext;

        private GraphicsDevice gd;

        private Swapchain sc;

        private Stopwatch sw;

        private double lastFrameTime;

        private CommandList eyesCL;

        private CommandList windowCL;

        public MirrorTextureEyeSource eyeSource = MirrorTextureEyeSource.BothEyes;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            engine.logger.Log("Graphics Backend:" + engine.backend.ToString(), true);
            if (engine.outputType == OutputType.Auto) {
                engine.outputType = OutputType.OculusVR;
                if (!VRContext.IsOculusSupported())
                {
                    engine.outputType = OutputType.SteamVR;
                    if (!VRContext.IsOpenVRSupported())
                    {
                        engine.logger.Log("Failed to find vr device starting in screen");
                        engine.outputType = OutputType.Screen;
                    }
                }
            }
            engine.logger.Log("Output Device:" + engine.outputType.ToString(), true);
            vrContext = buildVRContext();
            (gd, sc) = engine.windowManager.mainWindow.CreateScAndGD(vrContext, engine.backend);
            vrContext.Initialize(gd);
            sw = Stopwatch.StartNew();
            lastFrameTime = sw.Elapsed.TotalSeconds;
            windowCL = gd.ResourceFactory.CreateCommandList();
            eyesCL = gd.ResourceFactory.CreateCommandList();
            mainQueue = new RenderQueue();
            return this;
        }

        public VRContext buildVRContext()
        {
            VRContextOptions options = new VRContextOptions
            {
                EyeFramebufferSampleCount = TextureSampleCount.Count4
            };
            switch (engine.outputType)
            {
                case OutputType.Screen:
                    return VRContext.CreateScreen(options);
                    break;
                case OutputType.SteamVR:
                    return VRContext.CreateOpenVR(options);
                    break;
                case OutputType.OculusVR:
                    return VRContext.CreateOculus(options);
                    break;
                default:
                    return VRContext.CreateScreen(options);
                    break;
            }
        }

        private void BuildMainRenderQueue()
        {
            mainQueue.Clear();
            engine.worldManager.addToRenderQueue(mainQueue);
        }

        private void RenderEye(CommandList cl, Framebuffer fb,  Matrix4x4 proj, Matrix4x4 view)
        {
            cl.SetFramebuffer(fb);
            cl.ClearDepthStencil(1f);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            BuildMainRenderQueue();

//            mesh.Render(cl, new UBO(
//                proj,
//                view,
//                Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(0.5f, -1, -2f)));

        }


        public void switchVRContext(OutputType type)
        {
            if(type != engine.outputType)
            {
                engine.outputType = type;
                vrContext.Dispose();
                vrContext = buildVRContext();
                vrContext.Initialize(gd);
            }

        }

        public void Update()
        {
            double newFrameTime = sw.Elapsed.TotalSeconds;
            double deltaSeconds = newFrameTime - lastFrameTime;
            lastFrameTime = newFrameTime;

            windowCL.Begin();
            windowCL.SetFramebuffer(sc.Framebuffer);
            windowCL.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
            vrContext.RenderMirrorTexture(windowCL, sc.Framebuffer, (engine.outputType != OutputType.Screen)? eyeSource : MirrorTextureEyeSource.LeftEye);
            windowCL.End();
            gd.SubmitCommands(windowCL);
            gd.SwapBuffers(sc);

            HmdPoseState poses = vrContext.WaitForPoses();

            // Render Eyes
            eyesCL.Begin();
                eyesCL.PushDebugGroup("Left Eye");
                Matrix4x4 leftView = poses.CreateView(VREye.Left, _userPosition, -Vector3.UnitZ, Vector3.UnitY);
                RenderEye(eyesCL, vrContext.LeftEyeFramebuffer, poses.LeftEyeProjection, leftView);
                eyesCL.PopDebugGroup();
            if (engine.outputType != OutputType.Screen)
            {
                eyesCL.PushDebugGroup("Right Eye");
                Matrix4x4 rightView = poses.CreateView(VREye.Right, _userPosition, -Vector3.UnitZ, Vector3.UnitY);
                RenderEye(eyesCL, vrContext.RightEyeFramebuffer, poses.RightEyeProjection, rightView);
                eyesCL.PopDebugGroup();
            }


            eyesCL.End();
            gd.SubmitCommands(eyesCL);

            vrContext.SubmitFrame();
        }
    }
}
