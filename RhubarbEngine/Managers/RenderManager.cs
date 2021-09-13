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

namespace RhubarbEngine.Managers
{
	public class RenderManager : IManager
	{
        public float FieldOfView
        {
            get
            {
                return _engine.settingsObject.RenderSettings.DesktopRenderSettings.fov;
            }
        }

        public float AspectRatio
        {
            get
            {
                return _engine.windowManager.mainWindow.aspectRatio;
            }
        }

        public float nearPlaneDistance = 0.01f;

		public float farPlaneDistance = 1000f;

		private Engine _engine;

        private Matrix4x4 UserTrans
        {
            get
            {
                return (_engine.worldManager.FocusedWorld != null) ? _engine.worldManager.FocusedWorld.PlayerTrans : _engine.worldManager.privateOverlay.PlayerTrans;
            }
        }

        private RenderQueue _mainQueue;

		public VRContext vrContext;

		public GraphicsDevice gd;

		public Swapchain sc;

		private CommandList _eyesCL;

		public CommandList windowCL;

		public Skybox skybox;

		public TextureView nulview;

		public TextureView gridview;

		public TextureView rhubarbview;

		public TextureView solidview;

		public TextureView rhubarbSolidview;

		public TextureView[] cursors = new TextureView[50];

        public MirrorTextureEyeSource EyeSource
        {
            get
            {
                return _engine.settingsObject.VRSettings.renderEye;
            }
        }

        public IManager initialize(Engine _engine)
		{
			this._engine = _engine;
			var backend = this._engine.backend;
			if (this._engine.platformInfo.platform == PlatformInfo.Platform.OSX)
			{
				backend = GraphicsBackend.Metal;
			}
			if (backend == GraphicsBackend.Metal && this._engine.platformInfo.platform != PlatformInfo.Platform.OSX && this._engine.platformInfo.platform != PlatformInfo.Platform.iOS)
			{
				backend = GraphicsBackend.Vulkan;
			}
			if (backend == GraphicsBackend.Direct3D11 && this._engine.platformInfo.platform != PlatformInfo.Platform.Windows)
			{
				backend = GraphicsBackend.Vulkan;
			}
			this._engine.logger.Log("Graphics Backend:" + backend, true);
			if (this._engine.outputType == OutputType.Auto && this._engine.settingsObject.VRSettings.StartInVR)
			{
				this._engine.outputType = OutputType.OculusVR;
				if (!VRContext.IsOculusSupported())
				{
					this._engine.outputType = OutputType.SteamVR;
					if (!VRContext.IsOpenVRSupported())
					{
						this._engine.logger.Log("Failed to find vr device starting in screen");
						this._engine.outputType = OutputType.Screen;
					}
				}
			}
			else if (this._engine.outputType == OutputType.Auto)
			{
				this._engine.outputType = OutputType.Screen;
			}
			this._engine.logger.Log("Output Device:" + this._engine.outputType.ToString(), true);
			vrContext = BuildVRContext();
			(gd, sc) = this._engine.windowManager.mainWindow.CreateScAndGD(vrContext, backend);
			this._engine.backend = backend;
			vrContext.Initialize(gd);
			windowCL = gd.ResourceFactory.CreateCommandList();
			_eyesCL = gd.ResourceFactory.CreateCommandList();
			_mainQueue = new RenderQueue();
			this._engine.windowManager.mainWindow.window.Resized += Window_Resized;
			Window_Resized();
			var _texture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "nulltexture.jpg"), false, true).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
            nulview = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(_texture);
			var gridtexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "Grid.jpg"), true, true).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
            gridview = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(gridtexture);

			var rhubatexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "RhubarbVR2.png"), false, true).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
            rhubarbview = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(rhubatexture);

			var rhubatextures = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "RhubarbVR.png"), false, true).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
            rhubarbSolidview = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(rhubatextures);
			var solidTexture = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, RNumerics.Colorf.White), false).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
            solidview = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(solidTexture);
			var index = 0;
			foreach (Input.Cursors item in Enum.GetValues(typeof(RhubarbEngine.Input.Cursors)))
			{
				var tempTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "StaticAssets", "Cursors", item + ".png"), false, true).CreateDeviceTexture(this._engine.renderManager.gd, this._engine.renderManager.gd.ResourceFactory);
                cursors[index] = this._engine.renderManager.gd.ResourceFactory.CreateTextureView(tempTexture);
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
			if (_engine.settingsObject.RenderSettings.DesktopRenderSettings.auto)
			{
				sc.Resize((uint)_engine.windowManager.mainWindow.window.Width, (uint)_engine.windowManager.mainWindow.window.Height);
			}
			else
			{
				sc.Resize((uint)_engine.settingsObject.RenderSettings.DesktopRenderSettings.x, (uint)_engine.settingsObject.RenderSettings.DesktopRenderSettings.y);
			}
		}

		public VRContext BuildVRContext()
		{
			var options = new VRContextOptions
			{
				EyeFramebufferSampleCount = TextureSampleCount.Count1
			};
			switch (_engine.outputType)
			{
				case OutputType.Screen:
					return VRContext.CreateScreen(options, _engine);
				case OutputType.SteamVR:
					return VRContext.CreateOpenVR(options, (_engine.settingsObject.VRSettings.StartAsOverlay) ? Valve.VR.EVRApplicationType.VRApplication_Scene : Valve.VR.EVRApplicationType.VRApplication_Scene);
				case OutputType.OculusVR:
					return VRContext.CreateOculus(options);
				default:
					return VRContext.CreateScreen(options, _engine);
			}
		}

#pragma warning disable IDE0060 // Remove unused parameter
        private void BuildMainRenderQueue(Matrix4x4 leftp, Matrix4x4 leftv, Matrix4x4 rightp, Matrix4x4 rightv)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			_mainQueue.Clear();
			//When doing only left eye it seemed to be fine this might have problems of headsets with more fov
			var frustum = new RhubarbEngine.Utilities.BoundingFrustum(leftp);
			_engine.worldManager.AddToRenderQueue(_mainQueue, RemderLayers.normal_overlay_privateOverlay, frustum, leftv);
			_mainQueue.Order();
		}

		private void RenderEye(CommandList cl, Framebuffer fb, Matrix4x4 proj, Matrix4x4 view)
		{
			cl.SetFramebuffer(fb);
			cl.ClearDepthStencil(1f);
			cl.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
			skybox.Render(cl, fb, proj, view);
			foreach (var renderObj in _mainQueue.Renderables)
			{
				renderObj.Render(gd, cl, new UBO(
				proj,
				view,
				renderObj.Entity.GlobalTrans()));
			}
		}


		public void SwitchVRContext(OutputType type)
		{
			if ((type != _engine.outputType) || vrContext.Disposed)
			{
				_engine.logger.Log("Output Device Change:" + type.ToString());
				_engine.outputType = type;
				var oldvrContext = vrContext;
				vrContext = BuildVRContext();
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
				RenderNoneThreadedRenderObjectsInWorld(world);
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
			}
			catch
			{

			}
		}

		private void RenderRenderObjects()
		{
			foreach (var world in _engine.worldManager.worlds)
			{
				if (world.Focus != World.World.FocusLevel.Background)
				{
					RenderRenderObjectsInWorld(world);
				}
			}
		}

		public async Task Update()
		{
			if (vrContext.Disposed)
			{
				Console.WriteLine("Going to screen Cuz Disposed");
				SwitchVRContext(OutputType.Screen);
			}

			try
			{
				if (vrContext.GetType() == typeof(ScreenContext))
				{
					((ScreenContext)vrContext).UpdateInput();
				}
				if (!await Task.Run(RenderRenderObjects).TimeOut(1000))
                {
                    Logger.Log("Render Render Objects TimeOut", true);
                }

                gd.WaitForIdle();
				var poses = vrContext.WaitForPoses();
				var leftView = poses.CreateView(VREye.Left, UserTrans, -Vector3.UnitZ, Vector3.UnitY);
				var rightView = poses.CreateView(VREye.Right, UserTrans, -Vector3.UnitZ, Vector3.UnitY);
				BuildMainRenderQueue(poses.LeftEyeProjection, leftView, poses.RightEyeProjection, rightView);
				_eyesCL.Begin();
				if (_engine.backend is not (GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES))
				{
					_eyesCL.PushDebugGroup("Left Eye");
				}
				try
				{
					RenderEye(_eyesCL, vrContext.LeftEyeFramebuffer, poses.LeftEyeProjection, leftView);
				}
				catch (Exception e)
				{
					throw new Exception("Left Eye" + e.ToString());
				}
				_eyesCL.PopDebugGroup();
				if (_engine.outputType != OutputType.Screen)
				{
                    if (_engine.backend is not (GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES))
					{
						_eyesCL.PushDebugGroup("Right Eye");
					}
					try
					{
						RenderEye(_eyesCL, vrContext.RightEyeFramebuffer, poses.RightEyeProjection, rightView);
					}
					catch (Exception e)
					{
						throw new Exception("Right Eye" + e.ToString());
					}
					_eyesCL.PopDebugGroup();
				}


				_eyesCL.End();
				gd.SubmitCommands(_eyesCL);
				gd.WaitForIdle();

				vrContext.SubmitFrame();
			}
			catch (Exception e)
			{
				Console.WriteLine("Render Error " + e.ToString());
			}
			if (_engine.windowManager.mainWindowOpen)
			{
				windowCL.Begin();
				windowCL.SetFramebuffer(sc.Framebuffer);
				windowCL.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0.2f, 1f));
				vrContext.RenderMirrorTexture(windowCL, sc.Framebuffer, (_engine.outputType != OutputType.Screen) ? EyeSource : MirrorTextureEyeSource.LeftEye);
				windowCL.End();
				gd.SubmitCommands(windowCL);
				gd.SwapBuffers(sc);
				gd.WaitForIdle();
			}
		}
	}
}
