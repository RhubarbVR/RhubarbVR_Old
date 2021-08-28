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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Veldrid.ImageSharp;
using SixLabors.ImageSharp;

namespace RhubarbEngine.Managers
{
    public class RenderManager : IManager
    {
        public float fieldOfView => engine.settingsObject.RenderSettings.DesktopRenderSettings.fov;
        public float aspectRatio => engine.windowManager.mainWindow.aspectRatio;

        public float nearPlaneDistance = 0.01f;

        public float farPlaneDistance = 1000f;

        private Engine engine;

        private Matrix4x4 _userTrans => (engine.worldManager.focusedWorld != null)?engine.worldManager.focusedWorld.playerTrans: engine.worldManager.privateOverlay.playerTrans;

        private RenderQueue mainQueue;

        public VRContext vrContext;

        public GraphicsDevice gd;

        public Swapchain sc;

        private CommandList eyesCL;

        public CommandList windowCL;

        public Skybox skybox;

        public TextureView nulview;

        public TextureView gridview;

        public TextureView rhubarbview;

        public TextureView solidview;

        public TextureView rhubarbSolidview;

        public TextureView[] cursors = new TextureView[50];

        public MirrorTextureEyeSource eyeSource => engine.settingsObject.VRSettings.renderEye;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            GraphicsBackend backend = engine.backend;
            if(engine.platformInfo.platform == PlatformInfo.Platform.OSX)
            {
                backend = GraphicsBackend.Metal;
            }
            if ((backend == GraphicsBackend.Metal && engine.platformInfo.platform != PlatformInfo.Platform.OSX) && engine.platformInfo.platform != PlatformInfo.Platform.iOS)
            {
                backend = GraphicsBackend.Vulkan;
            }
            if (backend == GraphicsBackend.Direct3D11 && engine.platformInfo.platform != PlatformInfo.Platform.Windows)
            {
                backend = GraphicsBackend.Vulkan;
            }
            engine.logger.Log("Graphics Backend:" + backend, true);
            if (engine.outputType == OutputType.Auto && engine.settingsObject.VRSettings.StartInVR) {
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
            }else if(engine.outputType == OutputType.Auto)
            {
                engine.outputType = OutputType.Screen; 
            }
            engine.logger.Log("Output Device:" + engine.outputType.ToString(), true);
            vrContext = buildVRContext();
            (gd, sc) = engine.windowManager.mainWindow.CreateScAndGD(vrContext, backend);
            engine.backend = backend;
            vrContext.Initialize(gd);
            windowCL = gd.ResourceFactory.CreateCommandList();
            eyesCL = gd.ResourceFactory.CreateCommandList();
            mainQueue = new RenderQueue();
            engine.windowManager.mainWindow.window. Resized += Window_Resized;
            Window_Resized();
            var _texture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "nulltexture.jpg"), false, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            nulview = engine.renderManager.gd.ResourceFactory.CreateTextureView(_texture);
            var gridtexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "Grid.jpg"), true, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            gridview = engine.renderManager.gd.ResourceFactory.CreateTextureView(gridtexture);

            var rhubatexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "RhubarbVR2.png"), false, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            rhubarbview = engine.renderManager.gd.ResourceFactory.CreateTextureView(rhubatexture);

            var rhubatextures = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "RhubarbVR.png"), false, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            rhubarbSolidview = engine.renderManager.gd.ResourceFactory.CreateTextureView(rhubatextures);
            var solidTexture = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, g3.Colorf.White),false).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            solidview = engine.renderManager.gd.ResourceFactory.CreateTextureView(solidTexture);
            int index = 0;
            foreach (Input.Cursors item in Enum.GetValues(typeof(RhubarbEngine.Input.Cursors)))
            {
                var tempTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "Cursors", item + ".png"), false, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                cursors[index] = engine.renderManager.gd.ResourceFactory.CreateTextureView(tempTexture);
                index++;
            }

            skybox = new Skybox(
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_ft.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_bk.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_lf.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_rt.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_up.png")),
    Image.Load<Rgba32>(Path.Combine(AppContext.BaseDirectory, "skybox", "miramar_dn.png")));
            skybox.CreateDeviceObjects(gd, vrContext.LeftEyeFramebuffer.OutputDescription);

            return this;
        }

        private void Window_Resized()
        {
            if (engine.settingsObject.RenderSettings.DesktopRenderSettings.auto)
            {
                sc.Resize((uint)engine.windowManager.mainWindow.window.Width, (uint)engine.windowManager.mainWindow.window.Height);
            }
            else
            {
                sc.Resize((uint)engine.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)engine.settingsObject.RenderSettings.DesktopRenderSettings.y);
            }
        }

        public VRContext buildVRContext()
        {
            VRContextOptions options = new VRContextOptions
            {
                EyeFramebufferSampleCount = TextureSampleCount.Count1
            };
            switch (engine.outputType)
            {
                case OutputType.Screen:
                    return VRContext.CreateScreen(options,engine);
                case OutputType.SteamVR:
                    return VRContext.CreateOpenVR(options, (engine.settingsObject.VRSettings.StartAsOverlay)?Valve.VR.EVRApplicationType.VRApplication_Scene : Valve.VR.EVRApplicationType.VRApplication_Scene);
                case OutputType.OculusVR:
                    return VRContext.CreateOculus(options);
                default:
                    return VRContext.CreateScreen(options, engine);
            }
        }

        private void BuildMainRenderQueue()
        {
            mainQueue.Clear();
            engine.worldManager.addToRenderQueue(mainQueue, RemderLayers.normal_overlay_privateOverlay);
            mainQueue.Order();
        }

        private void RenderEye(CommandList cl, Framebuffer fb,  Matrix4x4 proj, Matrix4x4 view)
        {
            cl.SetFramebuffer(fb);
            cl.ClearDepthStencil(1f);
            cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            skybox.Render(cl, fb, proj, view);
            foreach (Renderable renderObj in mainQueue.Renderables)
            {
                renderObj.Render(gd, cl, new UBO(
                proj,
                view,
                renderObj.entity.globalTrans()));
            }
        }


        public void switchVRContext(OutputType type)
        {
            if(type != engine.outputType)
            {
                engine.logger.Log("Output Device Change:" + type.ToString());
                engine.outputType = type;
                var oldvrContext = vrContext;
                vrContext = buildVRContext();
                vrContext.Initialize(gd);
                if (!oldvrContext.Disposed)
                {
                    oldvrContext.Dispose();
                }
            }

        }

        private void RenderNoneThreadedRenderObjectsInWorld(World.World world)
        {
            try
            {
                foreach (var noneThreaded in world.updateLists.renderObject)
                {
                    try
                    {
                        noneThreaded.Render();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Failed To Render " + noneThreaded.GetType().Name + " Error " + e.ToString(), true);
                    }
                }
            }
            catch { }
        }

        private void RenderRenderObjectsInWorld(World.World world)
        {
            try
            {
                var noneThreadedTask = Task.Run(() => { RenderNoneThreadedRenderObjectsInWorld(world); });
                Parallel.ForEach(world.updateLists.trenderObject, obj =>
                {
                    try
                    {
                        obj.Render();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Failed To Render " + obj.GetType().Name + " Error " + e.ToString(), true);
                    }
                });
                noneThreadedTask.TimeOut(100).Wait();
            }
            catch 
            {

            }
        }

        private void RenderRenderObjects()
        {
            foreach (var world in engine.worldManager.worlds)
            {
                if(world.Focus != World.World.FocusLevel.Background)
                {
                    RenderRenderObjectsInWorld(world);
                }
            }
        }

        public void Update()
        {
            if (vrContext.Disposed)
            {
               switchVRContext(OutputType.Screen);
            }

            try
            {
                RenderRenderObjects();
                BuildMainRenderQueue();
                // Render Eyes
                HmdPoseState poses = vrContext.WaitForPoses();
                eyesCL.Begin();
                if (!(engine.backend == GraphicsBackend.OpenGL || engine.backend == GraphicsBackend.OpenGLES))
                {
                    eyesCL.PushDebugGroup("Left Eye");
                }
                Matrix4x4 leftView = poses.CreateView(VREye.Left, _userTrans, -Vector3.UnitZ, Vector3.UnitY);
                try
                {
                    RenderEye(eyesCL, vrContext.LeftEyeFramebuffer, poses.LeftEyeProjection, leftView);
                }
                catch (Exception e)
                {
                    throw new Exception("Left Eye" + e.ToString());
                }
                eyesCL.PopDebugGroup();
                if (engine.outputType != OutputType.Screen)
                {
                    if (!(engine.backend == GraphicsBackend.OpenGL || engine.backend == GraphicsBackend.OpenGLES))
                    {
                        eyesCL.PushDebugGroup("Right Eye");
                    }
                    Matrix4x4 rightView = poses.CreateView(VREye.Right, _userTrans, -Vector3.UnitZ, Vector3.UnitY);
                    try
                    {
                        RenderEye(eyesCL, vrContext.RightEyeFramebuffer, poses.RightEyeProjection, rightView);
                    }
                    catch (Exception e){
                        throw new Exception("Right Eye"+e.ToString());
                    }
                    eyesCL.PopDebugGroup();
                }


                eyesCL.End();
                gd.SubmitCommands(eyesCL);

                vrContext.SubmitFrame();
            }
            catch (Exception e)
            {
                Console.WriteLine("Render Error" + e.ToString());
            }
            if (engine.windowManager.mainWindowOpen)
            {
                windowCL.Begin();
                windowCL.SetFramebuffer(sc.Framebuffer);
                windowCL.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
                vrContext.RenderMirrorTexture(windowCL, sc.Framebuffer, (engine.outputType != OutputType.Screen) ? eyeSource : MirrorTextureEyeSource.LeftEye);
                windowCL.End();
                gd.SubmitCommands(windowCL);
                gd.SwapBuffers(sc);
            }
        }
    }
}
