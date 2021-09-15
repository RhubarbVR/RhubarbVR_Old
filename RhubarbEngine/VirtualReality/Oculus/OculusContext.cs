using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Veldrid;
using static RhubarbEngine.VirtualReality.Oculus.LibOvrNative;
using RhubarbEngine.Input.Controllers;

namespace RhubarbEngine.VirtualReality.Oculus
{
	internal unsafe class OculusContext : VRContext
	{
        private readonly ovrGraphicsLuid _luid;
		private readonly OculusMirrorTexture _mirrorTexture;
		private readonly VRContextOptions _options;
        private ovrHmdDesc _hmdDesc;
		private string _deviceName;
		private ovrRecti[] _eyeRenderViewport;
		private OculusSwapchain[] _eyeSwapchains;
		private int _frameIndex;
		private ovrTimewarpProjectionDesc _posTimewarpProjectionDesc;
		private double _sensorSampleTime;
		private EyePair_ovrPosef _eyeRenderPoses;
		private readonly Quaternion[] _rotations = new Quaternion[2];
		private readonly Vector3[] _positions = new Vector3[2];
		private readonly Matrix4x4[] _projections = new Matrix4x4[2];

		private static readonly Lazy<bool> _isSupported = new(CheckSupport);
		private static bool CheckSupport()
		{
			try
			{
                var initParams = new ovrInitParams
                {
                    Flags = OvrInitFlags.RequestVersion | OvrInitFlags.FocusAware | OvrInitFlags.Debug,
                    RequestedMinorVersion = 30
                };

                var result = ovr_Initialize(&initParams);
				if (result != OvrResult.Success)
				{
					return false;
				}

				ovrSession session;
				ovrGraphicsLuid luid;
				result = ovr_Create(&session, &luid);
				if (result != OvrResult.Success)
				{
					return false;
				}

				ovr_Destroy(session);
				ovr_Shutdown();
				return true;
			}
			catch
			{
				return false;
			}
		}

		internal static bool IsSupported() => _isSupported.Value;

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
                return _eyeSwapchains[0].GetFramebuffer();
            }
        }

        public override Framebuffer RightEyeFramebuffer
        {
            get
            {
                return _eyeSwapchains[1].GetFramebuffer();
            }
        }

        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal ovrSession Session { get; }

        public override IController LeftController
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IController RightController
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Matrix4x4 Headpos
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public OculusContext(VRContextOptions options)
		{
			_options = options;

            var initParams = new ovrInitParams
            {
                Flags = OvrInitFlags.RequestVersion | OvrInitFlags.FocusAware | OvrInitFlags.Debug,
                RequestedMinorVersion = 30
            };

            var result = ovr_Initialize(&initParams);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to initialize Oculus: {result}");
			}

			ovrSession session;
			ovrGraphicsLuid luid;
			result = ovr_Create(&session, &luid);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to create an Oculus session.");
			}

			Session = session;
			_luid = luid;

			_mirrorTexture = new OculusMirrorTexture(this);
		}

		public override void Initialize(GraphicsDevice gd)
		{
			GraphicsDevice = gd;
			if (gd.GetVulkanInfo(out var vkInfo))
			{
				IntPtr physicalDevice;
				var result = ovr_GetSessionPhysicalDeviceVk(Session, _luid, vkInfo.Instance, &physicalDevice);
				if (result != OvrResult.Success)
				{
					throw new VeldridException($"Failed to get Vulkan physical device.");
				}

				result = ovr_SetSynchonizationQueueVk(Session, vkInfo.GraphicsQueue);
				if (result != OvrResult.Success)
				{
					throw new VeldridException($"Failed to set the Oculus session's Vulkan synchronization queue.");
				}
			}

			_hmdDesc = ovr_GetHmdDesc(Session);
			_deviceName = _hmdDesc.ProductName.ToString();

			_eyeRenderViewport = new ovrRecti[2];
			_eyeSwapchains = new OculusSwapchain[2];
			for (var eye = 0; eye < 2; ++eye)
			{
				var idealSize = ovr_GetFovTextureSize(
					Session,
					(ovrEyeType)eye,
					_hmdDesc.DefaultEyeFov[eye],
					1.0f);
				_eyeSwapchains[eye] = new OculusSwapchain(
					GraphicsDevice,
					Session,
					idealSize.w, idealSize.h,
					Util.GetSampleCount(_options.EyeFramebufferSampleCount),
					createDepth: true);
				_eyeRenderViewport[eye].Pos.X = 0;
				_eyeRenderViewport[eye].Pos.Y = 0;
				_eyeRenderViewport[eye].Size = idealSize;
			}
		}

		public override void RenderMirrorTexture(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
		{
			_mirrorTexture.Render(cl, fb, source);
		}

		public override void SubmitFrame()
		{
			if (Disposed)
			{
				return;
			}
            if (GraphicsDevice.GetOpenGLInfo(out var glInfo))
			{
				glInfo.FlushAndFinish();
			}

			for (var eye = 0; eye < 2; ++eye)
			{
				_eyeSwapchains[eye].Commit();
			}

			// Initialize our single full screen Fov layer.
			var ld = new ovrLayerEyeFovDepth();
			ld.Header.Type = ovrLayerType.EyeFovDepth;
            ld.Header.Flags = GraphicsDevice.BackendType is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES
                ? ovrLayerFlags.TextureOriginAtBottomLeft
				: ovrLayerFlags.None;
			ld.ProjectionDesc = _posTimewarpProjectionDesc;
			ld.SensorSampleTime = _sensorSampleTime;

			for (var eye = 0; eye < 2; ++eye)
			{
				ld.ColorTexture[eye] = _eyeSwapchains[eye].ColorChain;
				ld.DepthTexture[eye] = _eyeSwapchains[eye].DepthChain;
				ld.Viewport[eye] = _eyeRenderViewport[eye];
				ld.Fov[eye] = _hmdDesc.DefaultEyeFov[eye];
				ld.RenderPose[eye] = _eyeRenderPoses[eye];
			}

			var layers = &ld.Header;
			var result = ovr_SubmitFrame(Session, _frameIndex, null, &layers, 1);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to submit Oculus frame: {result}");
			}

			_frameIndex++;
		}

		public unsafe override HmdPoseState WaitForPoses()
		{
			ovrSessionStatus sessionStatus;
			var result = ovr_GetSessionStatus(Session, &sessionStatus);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to retrieve Oculus session status: {result}");
			}

			if (sessionStatus.ShouldRecenter)
			{
				ovr_RecenterTrackingOrigin(Session);
			}

			// Call ovr_GetRenderDesc each frame to get the ovrEyeRenderDesc, as the returned values (e.g. HmdToEyePose) may change at runtime.
			var eyeRenderDescs = stackalloc ovrEyeRenderDesc[2];
			eyeRenderDescs[0] = ovr_GetRenderDesc2(Session, ovrEyeType.Left, _hmdDesc.DefaultEyeFov[0]);
			eyeRenderDescs[1] = ovr_GetRenderDesc2(Session, ovrEyeType.Right, _hmdDesc.DefaultEyeFov[1]);

			// Get both eye poses simultaneously, with IPD offset already included. 
			var hmdToEyePoses = new EyePair_ovrPosef(
				eyeRenderDescs[0].HmdToEyePose,
				eyeRenderDescs[1].HmdToEyePose);

			var predictedTime = ovr_GetPredictedDisplayTime(Session, _frameIndex);

			var trackingState = ovr_GetTrackingState(Session, predictedTime, true);

			double sensorSampleTime;    // sensorSampleTime is fed into the layer later
			var hmdToEyeOffset = new EyePair_Vector3(
				hmdToEyePoses.Left.Position,
				hmdToEyePoses.Right.Position);
			ovr_GetEyePoses(Session, _frameIndex, true, &hmdToEyeOffset, out _eyeRenderPoses, &sensorSampleTime);
			_sensorSampleTime = sensorSampleTime;

			// Render Scene to Eye Buffers
			for (var eye = 0; eye < 2; ++eye)
			{
				_rotations[eye] = _eyeRenderPoses[eye].Orientation;
				_positions[eye] = _eyeRenderPoses[eye].Position;
				var proj = ovrMatrix4f_Projection(eyeRenderDescs[eye].Fov, 0.2f, 1000f, ovrProjectionModifier.None);
				_posTimewarpProjectionDesc = ovrTimewarpProjectionDesc_FromProjection(proj, ovrProjectionModifier.None);
				_projections[eye] = Matrix4x4.Transpose(proj);
			}

			return new HmdPoseState(
				_projections[0], _projections[1],
				_positions[0], _positions[1],
				_rotations[0], _rotations[1]);
		}

		public override void Dispose()
		{
			Disposed = true;
			foreach (var sc in _eyeSwapchains)
			{
				sc.Dispose();
			}

			_mirrorTexture.Dispose();
			ovr_Destroy(Session);
			ovr_Shutdown();
		}

		public override (string[] instance, string[] device) GetRequiredVulkanExtensions()
		{
			uint instanceExtCount;
			var result = ovr_GetInstanceExtensionsVk(_luid, null, &instanceExtCount);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to retrieve the number of required Vulkan instance extensions: {result}");
			}

			var instanceExtensions = new byte[instanceExtCount];
			fixed (byte* instanceExtensionsPtr = &instanceExtensions[0])
			{
				result = ovr_GetInstanceExtensionsVk(_luid, instanceExtensionsPtr, &instanceExtCount);
				if (result != OvrResult.Success)
				{
					throw new VeldridException($"Failed to retrieve the required Vulkan instance extensions: {result}");
				}
			}

			var instance = GetStringArray(instanceExtensions);

			uint deviceExtCount;
			result = ovr_GetDeviceExtensionsVk(_luid, null, &deviceExtCount);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to retrieve the number of required Vulkan device extensions: {result}");
			}

			var deviceExtensions = new byte[deviceExtCount];
			fixed (byte* deviceExtensionsPtr = &deviceExtensions[0])
			{
				result = ovr_GetDeviceExtensionsVk(_luid, deviceExtensionsPtr, &deviceExtCount);
				if (result != OvrResult.Success)
				{
					throw new VeldridException($"Failed to retrieve the required Vulkan device extensions: {result}");
				}
			}

			var device = GetStringArray(deviceExtensions);

			return (instance, device);
		}

		private static string[] GetStringArray(byte[] utf8Data)
		{
			var ret = new List<string>();
			var start = 0;
			for (var i = 0; i < utf8Data.Length; i++)
			{
				if ((char)utf8Data[i] == ' ' || utf8Data[i] == 0)
				{
					var s = Encoding.UTF8.GetString(utf8Data, start, i - start);
					ret.Add(s);
					i += 1;
					start = i;
				}
			}

			return ret.ToArray();
		}
	}

	internal unsafe class OculusSwapchain
	{
		private static readonly Guid _d3d11Tex2DGuid = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

		private readonly ovrSession _session;
		public readonly ovrTextureSwapChain ColorChain;
		public readonly ovrTextureSwapChain DepthChain;
		public readonly Framebuffer[] Framebuffers;

		public OculusSwapchain(GraphicsDevice gd, ovrSession session, int sizeW, int sizeH, int sampleCount, bool createDepth)
		{
			_session = session;

			Texture[] colorTextures;
			Texture[] depthTextures = null;

            var colorDesc = new ovrTextureSwapChainDesc
            {
                Type = ovrTextureType.Texture2D,
                ArraySize = 1,
                Width = sizeW,
                Height = sizeH,
                MipLevels = 1,
                SampleCount = sampleCount,
                Format = ovrTextureFormat.R8G8B8A8_UNORM_SRGB,
                MiscFlags = ovrTextureMiscFlags.DX_Typeless | ovrTextureMiscFlags.AllowGenerateMips,
                BindFlags = ovrTextureBindFlags.DX_RenderTarget,
                StaticImage = false
            };

            (ColorChain, colorTextures) = CreateSwapchain(session, gd, colorDesc);

			// if requested, then create depth swap chain
			if (createDepth)
			{
                var depthDesc = new ovrTextureSwapChainDesc
                {
                    Type = ovrTextureType.Texture2D,
                    ArraySize = 1,
                    Width = sizeW,
                    Height = sizeH,
                    MipLevels = 1,
                    SampleCount = sampleCount,
                    Format = ovrTextureFormat.D32_FLOAT,
                    MiscFlags = ovrTextureMiscFlags.None,
                    BindFlags = ovrTextureBindFlags.DX_DepthStencil,
                    StaticImage = false
                };

                (DepthChain, depthTextures) = CreateSwapchain(session, gd, depthDesc);
			}

			Framebuffers = new Framebuffer[colorTextures.Length];
			for (var i = 0; i < Framebuffers.Length; i++)
			{
				Framebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
					depthTextures?[i],
					colorTextures[i]));
			}

			CommandList = gd.ResourceFactory.CreateCommandList();
		}

		private (ovrTextureSwapChain, Texture[]) CreateSwapchain(
			ovrSession session,
			GraphicsDevice gd,
			ovrTextureSwapChainDesc desc)
		{
            return gd.BackendType switch
            {
                GraphicsBackend.Direct3D11 => CreateSwapchainD3D11(session, gd, desc),
                GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES => CreateSwapchainGL(session, gd, desc),
                GraphicsBackend.Vulkan => CreateSwapchainVk(session, gd, desc),
                GraphicsBackend.Metal => throw new PlatformNotSupportedException("Using Oculus with the Metal backend is not supported."),
                _ => throw new NotImplementedException(),
            };
        }

		private (ovrTextureSwapChain, Texture[]) CreateSwapchainVk(ovrSession session, GraphicsDevice gd, ovrTextureSwapChainDesc desc)
		{
			ovrTextureSwapChain otsc;
			Texture[] textures;

			var result = ovr_CreateTextureSwapChainVk(session, gd.GetVulkanInfo().Device, &desc, &otsc);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to call ovr_CreateTextureSwapChainVk: {result}");
			}

			var textureCount = 0;
			ovr_GetTextureSwapChainLength(session, otsc, &textureCount);
			textures = new Texture[textureCount];
			for (var i = 0; i < textureCount; ++i)
			{
				ulong nativeTexture;
				ovr_GetTextureSwapChainBufferVk(session, otsc, i, &nativeTexture);
				textures[i] = gd.ResourceFactory.CreateTexture(
					nativeTexture,
					OculusUtil.GetVeldridTextureDescription(desc));
			}

			return (otsc, textures);
		}

		private static (ovrTextureSwapChain, Texture[]) CreateSwapchainD3D11(
			ovrSession session,
			GraphicsDevice gd,
			ovrTextureSwapChainDesc desc)
		{
			ovrTextureSwapChain otsc;
			Texture[] textures;

			var result = ovr_CreateTextureSwapChainDX(session, gd.GetD3D11Info().Device, &desc, &otsc);
			if (result != OvrResult.Success)
			{
				throw new VeldridException($"Failed to call ovr_CreateTextureSwapChainDX: {result}");
			}

			var textureCount = 0;
			ovr_GetTextureSwapChainLength(session, otsc, &textureCount);
			textures = new Texture[textureCount];
			for (var i = 0; i < textureCount; ++i)
			{
				IntPtr nativeTexture;
				ovr_GetTextureSwapChainBufferDX(session, otsc, i, _d3d11Tex2DGuid, &nativeTexture);
				textures[i] = gd.ResourceFactory.CreateTexture(
					(ulong)nativeTexture,
					OculusUtil.GetVeldridTextureDescription(desc));
			}

			return (otsc, textures);
		}

		private static (ovrTextureSwapChain, Texture[]) CreateSwapchainGL(
			ovrSession session,
			GraphicsDevice gd,
			ovrTextureSwapChainDesc desc)
		{
			ovrTextureSwapChain otsc = default;
			Texture[] textures = default;

			var result = OvrResult.Success;
			gd.GetOpenGLInfo().ExecuteOnGLThread(() =>
			{
				var localDesc = desc;
				localDesc.MiscFlags = localDesc.MiscFlags & ~(ovrTextureMiscFlags.DX_Typeless | ovrTextureMiscFlags.AllowGenerateMips);
				localDesc.BindFlags = ovrTextureBindFlags.None;

				ovrTextureSwapChain sc;
				result = ovr_CreateTextureSwapChainGL(session, &localDesc, &sc);

				if (result != OvrResult.Success)
				{
					return;
				}
				otsc = sc;
			});

			if (otsc.IsNull)
			{
				throw new VeldridException($"Failed to call ovr_CreateTextureSwapChainGL: {result}");
			}

			var textureCount = 0;
			ovr_GetTextureSwapChainLength(session, otsc, &textureCount);
			textures = new Texture[textureCount];
			for (var i = 0; i < textureCount; ++i)
			{
				uint glID;
				ovr_GetTextureSwapChainBufferGL(session, otsc, i, &glID);
				textures[i] = gd.ResourceFactory.CreateTexture(
					glID,
					OculusUtil.GetVeldridTextureDescription(desc));
			}

			return (otsc, textures);
		}

		public void Dispose()
		{
			foreach (var fb in Framebuffers)
			{
				fb.Dispose();
			}

			if (ColorChain.NativePtr != IntPtr.Zero)
			{
				ovr_DestroyTextureSwapChain(_session, ColorChain);
			}
			if (DepthChain.NativePtr != IntPtr.Zero)
			{
				ovr_DestroyTextureSwapChain(_session, DepthChain);
			}
		}

		public Framebuffer GetFramebuffer()
		{
			var index = 0;
			ovr_GetTextureSwapChainCurrentIndex(_session, ColorChain, &index);
			return Framebuffers[index];
		}

		public OutputDescription GetOutputDescription() => Framebuffers[0].OutputDescription;

		public CommandList CommandList { get; private set; }

		public void Commit()
		{
			var result = ovr_CommitTextureSwapChain(_session, ColorChain);
			if (result != OvrResult.Success)
			{ throw new InvalidOperationException(); }

			result = ovr_CommitTextureSwapChain(_session, DepthChain);
			if (result != OvrResult.Success)
			{ throw new InvalidOperationException(); }
		}
	}
}
