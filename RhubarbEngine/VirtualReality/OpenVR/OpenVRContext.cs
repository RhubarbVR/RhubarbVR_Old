using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Valve.VR;
using Veldrid;
using Veldrid.Vk;
using OVR = Valve.VR.OpenVR;
using RhubarbEngine.VirtualReality.OpenVR.Controllers;
using System.Runtime.InteropServices;
using RhubarbEngine.Input.Controllers;

namespace RhubarbEngine.VirtualReality.OpenVR
{
	internal class OpenVRContext : VRContext
	{
		private readonly CVRSystem _vrSystem;
		private readonly CVRCompositor _compositor;
		private readonly OpenVRMirrorTexture _mirrorTexture;
		private readonly VRContextOptions _options;
        private string _deviceName;
		private Framebuffer _leftEyeFB;
		private Framebuffer _rightEyeFB;
		private Matrix4x4 _projLeft;
		private Matrix4x4 _projRight;
		private Matrix4x4 _headToEyeLeft;
		private Matrix4x4 _headToEyeRight;
		private readonly TrackedDevicePose_t[] _devicePoses = new TrackedDevicePose_t[1];

		public VRActiveActionSet_t generalActionSet;
		public VRActiveActionSet_t viveActionSet;
		public VRActiveActionSet_t cosmosActionSet;
		public VRActiveActionSet_t knucklesActionSet;
		public VRActiveActionSet_t oculustouchActionSet;

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
                return _rightEyeFB;
            }
        }

        internal GraphicsDevice GraphicsDevice { get; private set; }

        public override IController LeftController
        {
            get
            {
                return (controllerOne != null) ? (controllerOne.Creality == Input.Creality.Left) ? controllerOne : controllerTwo : null;
            }
        }

        public override IController RightController
        {
            get
            {
                return (controllerOne != null) ? (controllerOne.Creality == Input.Creality.Right) ? controllerOne : controllerTwo : null;
            }
        }

        public override Matrix4x4 Headpos
        {
            get
            {
                return _headPos;
            }
        }

        public static Matrix4x4 InvertRot(Matrix4x4 val)
		{
			Matrix4x4.Decompose(val, out var scale, out var rot, out var trans);

			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(rot)) * Matrix4x4.CreateTranslation(trans);
		}
		public static Quaternion QuaternionFromMatrix(HmdMatrix34_t m)
		{
			var w = Math.Sqrt(1 + m.m0 + m.m5 + m.m10) / 2.0;
			return new Quaternion
			{
				W = (float)w, // Scalar
				X = (float)((m.m9 - m.m6) / (4 * w)),
				Y = (float)((m.m2 - m.m8) / (4 * w)),
				Z = (float)((m.m4 - m.m1) / (4 * w))
			};
		}


		public static Matrix4x4 PosHelp(HmdMatrix34_t pos)
		{
			return Matrix4x4.CreateScale(1) * Matrix4x4.CreateFromQuaternion(QuaternionFromMatrix(pos)) * Matrix4x4.CreateTranslation(new Vector3(pos.m3, pos.m7, pos.m11));
		}

		public OpenVRContext(VRContextOptions options, EVRApplicationType e = EVRApplicationType.VRApplication_Scene)
		{
			_options = options;
			var initError = EVRInitError.None;

			_vrSystem = OVR.Init(ref initError, e, OVR.k_pch_SteamVR_NeverKillProcesses_Bool + "true");
			if (initError != EVRInitError.None)
			{
				throw new VeldridException($"Failed to initialize OpenVR: {OVR.GetStringForHmdError(initError)}");
			}
			_compositor = OVR.Compositor;
			if (_compositor == null)
			{
				throw new VeldridException("Failed to access the OpenVR Compositor.");
			}

			Logger.Log("Loading app.vrmanifest");
            var apperro = OVR.Applications.AddApplicationManifest(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.vrmanifest"), false);
            if (apperro != EVRApplicationError.None)
            {
                Logger.Log($"Failed to load Application Manifest: {Enum.GetName(typeof(EVRApplicationError), apperro)}", true);
            }
            else
            {
                Logger.Log("Application manifest loaded successfully.");
            }

            _mirrorTexture = new OpenVRMirrorTexture(this);
			var error = OVR.Input.SetActionManifestPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SteamVR", "steamvr_manifest.json"));
			if (error != EVRInputError.None)
			{
				Logger.Log($"Action manifest error {error}");
			}

			if (error != EVRInputError.None)
			{
				Logger.Log($"Action Get Action Handl error {error}");
			}
			generalActionSet = SetUPActionSet("/actions/General");
			viveActionSet = SetUPActionSet("/actions/HTCVive");
			cosmosActionSet = SetUPActionSet("/actions/Cosmos");
			knucklesActionSet = SetUPActionSet("/actions/Knuckles");
			oculustouchActionSet = SetUPActionSet("/actions/OculusTouch");
		}

		public SteamVRController controllerOne;

		public SteamVRController controllerTwo;
		ulong _leftHandle = 0;
		ulong _rightHandle = 0;

		private SteamVRController SetupSteamVRController(string divisenamen, uint devicetackindex)
		{
			var role = OVR.System.GetControllerRoleForTrackedDeviceIndex(devicetackindex);
			switch (role)
			{
				case ETrackedControllerRole.Invalid:
					break;
				case ETrackedControllerRole.LeftHand:
					return new SteamVRController(this, divisenamen, devicetackindex, Input.Creality.Left, _leftHandle);
				case ETrackedControllerRole.RightHand:
					return new SteamVRController(this, divisenamen, devicetackindex, Input.Creality.Right, _rightHandle);
				case ETrackedControllerRole.OptOut:
					break;
				case ETrackedControllerRole.Max:
					break;
				default:
					break;
			}
			return null;
		}

		public void UpdateControllers()
		{
			controllerOne = null;
			controllerTwo = null;
			OVR.Input.GetInputSourceHandle("/user/hand/left", ref _leftHandle);
			OVR.Input.GetInputSourceHandle("/user/hand/right", ref _rightHandle);

			Logger.Log($"Left: {_leftHandle} Right: {_rightHandle}");
			for (uint i = 0; i < OVR.k_unMaxTrackedDeviceCount; i++)
			{
				var device = OVR.System.GetTrackedDeviceClass(i);
				if (device == ETrackedDeviceClass.Controller)
				{
					var error = ETrackedPropertyError.TrackedProp_Success;
					var value = new StringBuilder(64);
					OVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, value, 64, ref error);
					if (controllerOne == null)
					{
						controllerOne = SetupSteamVRController(value.ToString(), i);
					}
					else if (controllerTwo == null)
					{

						controllerTwo = SetupSteamVRController(value.ToString(), i);
					}
					else
					{
						goto SetupTracker;
					}
				}
				else if (device == ETrackedDeviceClass.GenericTracker)
				{
					goto SetupTracker;
				}
				else
				{
					goto Done;
				}
			SetupTracker:

			Done:
				;
			}

		}

		private static VRActiveActionSet_t SetUPActionSet(string path)
		{
			ulong handle = 0;
			var error = OVR.Input.GetActionSetHandle(path, ref handle);
			if (error != EVRInputError.None)
			{
				Logger.Log($"Action Set Handle  {path}  error {error}");
			}
			var actionSet = new VRActiveActionSet_t
			{
				ulActionSet = handle,
				ulRestrictedToDevice = 0,
				nPriority = 0
			};
			return actionSet;
		}

		internal static bool IsSupported()
		{
			try
			{
				return OVR.IsHmdPresent();
			}
			catch
			{

				return false;
			}
		}

		public override void Initialize(GraphicsDevice gd)
		{
			GraphicsDevice = gd;

			var sb = new StringBuilder(512);
			var error = ETrackedPropertyError.TrackedProp_Success;
			_vrSystem.GetStringTrackedDeviceProperty(
				OVR.k_unTrackedDeviceIndex_Hmd,
				ETrackedDeviceProperty.Prop_TrackingSystemName_String,
				sb,
				512u,
				ref error);
			_deviceName = error != ETrackedPropertyError.TrackedProp_Success ? "<Unknown OpenVR Device>" : sb.ToString();
            Logger.Log("Head Set Type: " + _deviceName);

			uint eyeWidth = 0;
			uint eyeHeight = 0;
			_vrSystem.GetRecommendedRenderTargetSize(ref eyeWidth, ref eyeHeight);

			_leftEyeFB = CreateFramebuffer(eyeWidth, eyeHeight);
			_rightEyeFB = CreateFramebuffer(eyeWidth, eyeHeight);

			var eyeToHeadLeft = ToSysMatrix(_vrSystem.GetEyeToHeadTransform(EVREye.Eye_Left));
			Matrix4x4.Invert(eyeToHeadLeft, out _headToEyeLeft);

			var eyeToHeadRight = ToSysMatrix(_vrSystem.GetEyeToHeadTransform(EVREye.Eye_Right));
			Matrix4x4.Invert(eyeToHeadRight, out _headToEyeRight);

			_projLeft = ToSysMatrix(_vrSystem.GetProjectionMatrix(EVREye.Eye_Left, 0.1f, 1000f));
			_projRight = ToSysMatrix(_vrSystem.GetProjectionMatrix(EVREye.Eye_Right, 0.1f, 1000f));


			try
			{
				UpdateControllers();
			}
			catch
			{

			}
		}



		public unsafe void UpdateInput()
		{
			var error = OVR.Input.UpdateActionState(new[] { generalActionSet, viveActionSet, cosmosActionSet, oculustouchActionSet, knucklesActionSet }, (uint)Marshal.SizeOf(typeof(VRActiveActionSet_t)));

			if (error != EVRInputError.None)
			{
				Logger.Log($"Input error {error}");
			}
			try
			{
				controllerOne?.Update();
			}
			catch
			{

			}
			try
			{
				controllerTwo?.Update();
			}
			catch
			{

			}
		}

		public override (string[] instance, string[] device) GetRequiredVulkanExtensions()
		{
			return (Array.Empty<string>(), Array.Empty<string>());
		}

		Matrix4x4 _headPos;
		public override HmdPoseState WaitForPoses()
		{
			if (Disposed)
			{
				return default;
			}
			var compositorError = _compositor.WaitGetPoses(_devicePoses, Array.Empty<TrackedDevicePose_t>());
			if (compositorError != EVRCompositorError.None)
			{
				throw new Exception($"Failed to WaitGetPoses from OpenVR: {compositorError}");
			}
			var hmdPose = _devicePoses[OVR.k_unTrackedDeviceIndex_Hmd];
			_headPos = PosHelp(hmdPose.mDeviceToAbsoluteTracking);

			var deviceToAbsolute = ToSysMatrix(hmdPose.mDeviceToAbsoluteTracking);
			Matrix4x4.Invert(deviceToAbsolute, out var absoluteToDevice);

			var viewLeft = absoluteToDevice * _headToEyeLeft;
			var viewRight = absoluteToDevice * _headToEyeRight;

			Matrix4x4.Invert(viewLeft, out var invViewLeft);
			Matrix4x4.Decompose(invViewLeft, out _, out var leftRotation, out var leftPosition);

			Matrix4x4.Invert(viewRight, out var invViewRight);
			Matrix4x4.Decompose(invViewRight, out _, out var rightRotation, out var rightPosition);

			return new HmdPoseState(
				_projLeft, _projRight,
				leftPosition, rightPosition,
				leftRotation, rightRotation);
		}

		public bool PollNextEvent(ref VREvent_t pEvent)
		{
			var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
			return _vrSystem.PollNextEvent(ref pEvent, size);
		}

		public override void SubmitFrame()
		{
			if (Disposed)
			{
				return;
			}
			var _vrevent = new VREvent_t();
			if (PollNextEvent(ref _vrevent))
			{
				if (_vrevent.eventType == 700)
				{
					_vrSystem.AcknowledgeQuit_Exiting();
					Dispose();
					return;
				}
                if (_vrevent.eventType is >= 100 and <= 102)
				{
					Task.Run(() =>
					{
						Task.Delay(1500);
						UpdateControllers();
					});
				}
			}
			if (GraphicsDevice.GetOpenGLInfo(out var glInfo))
			{
				glInfo.FlushAndFinish();
			}
			UpdateInput();
			SubmitTexture(_compositor, LeftEyeFramebuffer.ColorTargets[0].Target, EVREye.Eye_Left);
			SubmitTexture(_compositor, RightEyeFramebuffer.ColorTargets[0].Target, EVREye.Eye_Right);
		}

		public override void RenderMirrorTexture(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
		{
			if (Disposed)
			{
				return;
			}
			_mirrorTexture.Render(cl, fb, source);
		}

		private void SubmitTexture(CVRCompositor compositor, Texture colorTex, EVREye eye)
		{
			if (Disposed)
			{
				return;
			}
			Texture_t texT;

			if (GraphicsDevice.GetD3D11Info(out var d3dInfo))
			{
				texT.eColorSpace = EColorSpace.Gamma;
				texT.eType = ETextureType.DirectX;
				texT.handle = d3dInfo.GetTexturePointer(colorTex);
			}
			else if (GraphicsDevice.GetOpenGLInfo(out var openglInfo))
			{
				texT.eColorSpace = EColorSpace.Gamma;
				texT.eType = ETextureType.OpenGL;
				texT.handle = (IntPtr)openglInfo.GetTextureName(colorTex);
			}
			else if (GraphicsDevice.GetVulkanInfo(out var vkInfo))
			{
				vkInfo.TransitionImageLayout(colorTex, (uint)Vulkan.VkImageLayout.TransferSrcOptimal);

				VRVulkanTextureData_t vkTexData;
				vkTexData.m_nImage = vkInfo.GetVkImage(colorTex);
				vkTexData.m_pDevice = vkInfo.Device;
				vkTexData.m_pPhysicalDevice = vkInfo.PhysicalDevice;
				vkTexData.m_pInstance = vkInfo.Instance;
				vkTexData.m_pQueue = vkInfo.GraphicsQueue;
				vkTexData.m_nQueueFamilyIndex = vkInfo.GraphicsQueueFamilyIndex;
				vkTexData.m_nWidth = colorTex.Width;
				vkTexData.m_nHeight = colorTex.Height;
				vkTexData.m_nFormat = (uint)VkFormats.VdToVkPixelFormat(
					colorTex.Format,
					(colorTex.Usage & TextureUsage.DepthStencil) != 0);
				vkTexData.m_nSampleCount = GetSampleCount(colorTex.SampleCount);

				texT.eColorSpace = EColorSpace.Gamma;
				texT.eType = ETextureType.Vulkan;
				unsafe
				{
					texT.handle = (IntPtr)(&vkTexData);
				}
			}
			else
			{
				throw new NotSupportedException();
			}

			VRTextureBounds_t boundsT;
			boundsT.uMin = 0;
			boundsT.uMax = 1;
			boundsT.vMin = 0;
			boundsT.vMax = 1;

			var compositorError = EVRCompositorError.None;
			if (GraphicsDevice.GetOpenGLInfo(out var glInfo))
			{
				glInfo.ExecuteOnGLThread(() => compositorError = compositor.Submit(eye, ref texT, ref boundsT, EVRSubmitFlags.Submit_Default));
			}
			else
			{
				compositorError = compositor.Submit(eye, ref texT, ref boundsT, EVRSubmitFlags.Submit_Default);
			}

			if (compositorError != EVRCompositorError.None)
			{
				throw new VeldridException($"Failed to submit to the OpenVR Compositor: {compositorError}");
			}
		}

		public override void Dispose()
		{
			Disposed = true;
			_mirrorTexture.Dispose();

			_leftEyeFB.ColorTargets[0].Target.Dispose();
			_leftEyeFB.DepthTarget?.Target.Dispose();
			_leftEyeFB.Dispose();

			_rightEyeFB.ColorTargets[0].Target.Dispose();
			_rightEyeFB.DepthTarget?.Target.Dispose();
			_rightEyeFB.Dispose();

			OVR.Shutdown();
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

		private static Matrix4x4 ToSysMatrix(HmdMatrix34_t hmdMat)
		{
			return new Matrix4x4(
				hmdMat.m0, hmdMat.m4, hmdMat.m8, 0f,
				hmdMat.m1, hmdMat.m5, hmdMat.m9, 0f,
				hmdMat.m2, hmdMat.m6, hmdMat.m10, 0f,
				hmdMat.m3, hmdMat.m7, hmdMat.m11, 1f);
		}

		private static Matrix4x4 ToSysMatrix(HmdMatrix44_t hmdMat)
		{
			return new Matrix4x4(
				hmdMat.m0, hmdMat.m4, hmdMat.m8, hmdMat.m12,
				hmdMat.m1, hmdMat.m5, hmdMat.m9, hmdMat.m13,
				hmdMat.m2, hmdMat.m6, hmdMat.m10, hmdMat.m14,
				hmdMat.m3, hmdMat.m7, hmdMat.m11, hmdMat.m15);
		}

		private static uint GetSampleCount(TextureSampleCount sampleCount)
		{
			switch (sampleCount)
			{
				case TextureSampleCount.Count1:
					return 1;
				case TextureSampleCount.Count2:
					return 2;
				case TextureSampleCount.Count4:
					return 4;
				case TextureSampleCount.Count8:
					return 8;
				case TextureSampleCount.Count16:
					return 16;
				case TextureSampleCount.Count32:
					return 32;
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
