using System;
using System.Collections.Generic;
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
        private GraphicsDevice _gd;
        private string _deviceName;
        private Framebuffer _leftEyeFB;
        private Framebuffer _rightEyeFB;
        private Matrix4x4 _projLeft;
        private Matrix4x4 _projRight;
        private Matrix4x4 _headToEyeLeft;
        private Matrix4x4 _headToEyeRight;
        private TrackedDevicePose_t[] _devicePoses = new TrackedDevicePose_t[1];

        public VRActiveActionSet_t generalActionSet;
        public VRActiveActionSet_t viveActionSet;
        public VRActiveActionSet_t cosmosActionSet;
        public VRActiveActionSet_t knucklesActionSet;
        public VRActiveActionSet_t oculustouchActionSet;

        public override string DeviceName => _deviceName;
        public override Framebuffer LeftEyeFramebuffer => _leftEyeFB;

        public override Framebuffer RightEyeFramebuffer => _rightEyeFB;

        internal GraphicsDevice GraphicsDevice => _gd;

        public override IController leftController => (controllerOne != null) ?(controllerOne.Creality == Input.Creality.Left)? controllerOne: controllerTwo:null;

        public override IController RightController => (controllerOne != null) ? (controllerOne.Creality == Input.Creality.Right) ? controllerOne : controllerTwo : null;

        public override Matrix4x4 Headpos => headPos;

        public Matrix4x4 invertRot(Matrix4x4 val)
        {
            Matrix4x4.Decompose(val, out Vector3 scale, out Quaternion rot, out Vector3 trans);

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


        public static Matrix4x4 posHelp(HmdMatrix34_t pos)
        {
            return Matrix4x4.CreateScale(1) * Matrix4x4.CreateFromQuaternion(QuaternionFromMatrix(pos)) * Matrix4x4.CreateTranslation(new Vector3(pos.m3, pos.m7, pos.m11));
        }

        public OpenVRContext(VRContextOptions options)
        {
            _options = options;
            EVRInitError initError = EVRInitError.None;

            _vrSystem = OVR.Init(ref initError, EVRApplicationType.VRApplication_Scene, OVR.k_pch_SteamVR_NeverKillProcesses_Bool + "true");
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
            EVRApplicationError apperro = EVRApplicationError.None;
            apperro = OVR.Applications.AddApplicationManifest(AppDomain.CurrentDomain.BaseDirectory + @"\\app.vrmanifest", false);
            if (apperro != EVRApplicationError.None) Logger.Log($"Failed to load Application Manifest: {Enum.GetName(typeof(EVRApplicationError), apperro)}", true);
            else Logger.Log("Application manifest loaded successfully.");

            _mirrorTexture = new OpenVRMirrorTexture(this);
            EVRInputError error = OVR.Input.SetActionManifestPath(AppDomain.CurrentDomain.BaseDirectory + @"\\SteamVR\\steamvr_manifest.json");
            if (error != EVRInputError.None)
            {
                Logger.Log($"Action manifest error {error.ToString()}");
            }

            if (error != EVRInputError.None)
            {
                Logger.Log($"Action Get Action Handl error {error.ToString()}");
            }
            generalActionSet = SetUPActionSet("/actions/General");
            viveActionSet = SetUPActionSet("/actions/HTCVive");
            cosmosActionSet = SetUPActionSet("/actions/Cosmos");
            knucklesActionSet = SetUPActionSet("/actions/Knuckles");
            oculustouchActionSet = SetUPActionSet("/actions/OculusTouch");
        }

        public SteamVRController controllerOne;

        public SteamVRController controllerTwo;
        ulong leftHandle = 0;
        ulong rightHandle = 0;

        private SteamVRController setupSteamVRController(string divisenamen,uint devicetackindex)
        {
            ETrackedControllerRole role = OVR.System.GetControllerRoleForTrackedDeviceIndex(devicetackindex);
            switch (role)
            {
                case ETrackedControllerRole.Invalid:
                    break;
                case ETrackedControllerRole.LeftHand:
                    return new SteamVRController(this, divisenamen, devicetackindex, Input.Creality.Left, leftHandle);
                    break;
                case ETrackedControllerRole.RightHand:
                    return new SteamVRController(this, divisenamen, devicetackindex, Input.Creality.Right, rightHandle);
                    break;
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
            OVR.Input.GetInputSourceHandle("/user/hand/left", ref leftHandle);
            OVR.Input.GetInputSourceHandle("/user/hand/right", ref rightHandle);

            Logger.Log($"Left: {leftHandle} Right: {rightHandle}");
            for (uint i = 0; i < OVR.k_unMaxTrackedDeviceCount; i++)
            {
                ETrackedDeviceClass device = OVR.System.GetTrackedDeviceClass(i);
                if (device == ETrackedDeviceClass.Controller)
                {
                    ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                    StringBuilder value = new StringBuilder(64);
                    OVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, value, 64, ref error);
                    if (controllerOne == null)
                    {
                        controllerOne = setupSteamVRController(value.ToString(),i);
                    }
                    else if (controllerTwo == null)
                    {

                        controllerTwo = setupSteamVRController(value.ToString(), i);
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

        private VRActiveActionSet_t SetUPActionSet(string path)
        {
            ulong handle = 0;
            EVRInputError error = OVR.Input.GetActionSetHandle(path, ref handle);
            if (error != EVRInputError.None)
            {
                Logger.Log($"Action Set Handle  {path}  error {error.ToString()}");
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
            _gd = gd;

            StringBuilder sb = new StringBuilder(512);
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            uint ret = _vrSystem.GetStringTrackedDeviceProperty(
                OVR.k_unTrackedDeviceIndex_Hmd,
                ETrackedDeviceProperty.Prop_TrackingSystemName_String,
                sb,
                512u,
                ref error);
            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                _deviceName = "<Unknown OpenVR Device>";
            }
            else
            {
                _deviceName = sb.ToString();
            }

            uint eyeWidth = 0;
            uint eyeHeight = 0;
            _vrSystem.GetRecommendedRenderTargetSize(ref eyeWidth, ref eyeHeight);

            _leftEyeFB = CreateFramebuffer(eyeWidth, eyeHeight);
            _rightEyeFB = CreateFramebuffer(eyeWidth, eyeHeight);

            Matrix4x4 eyeToHeadLeft = ToSysMatrix(_vrSystem.GetEyeToHeadTransform(EVREye.Eye_Left));
            Matrix4x4.Invert(eyeToHeadLeft, out _headToEyeLeft);

            Matrix4x4 eyeToHeadRight = ToSysMatrix(_vrSystem.GetEyeToHeadTransform(EVREye.Eye_Right));
            Matrix4x4.Invert(eyeToHeadRight, out _headToEyeRight);

            _projLeft = ToSysMatrix(_vrSystem.GetProjectionMatrix(EVREye.Eye_Left, 0.1f, 1000f));
            _projRight = ToSysMatrix(_vrSystem.GetProjectionMatrix(EVREye.Eye_Right, 0.1f, 1000f));



            UpdateControllers();
        }



        public unsafe void updateInput()
        {
            EVRInputError error = OVR.Input.UpdateActionState(new[] { generalActionSet, viveActionSet, cosmosActionSet, oculustouchActionSet, knucklesActionSet }, (uint)Marshal.SizeOf(typeof(VRActiveActionSet_t)));

            if (error != EVRInputError.None)
            {
                Logger.Log($"Input error {error.ToString()}");
            }
            controllerOne?.update();
            controllerTwo?.update();
        }

        public override (string[] instance, string[] device) GetRequiredVulkanExtensions()
        {
            StringBuilder sb = new StringBuilder(1024);
            uint ret = _compositor.GetVulkanInstanceExtensionsRequired(sb, 1024);
            string[] instance = sb.ToString().Split(' ');
            sb.Clear();
            ret = _compositor.GetVulkanDeviceExtensionsRequired(IntPtr.Zero, sb, 1024);
            string[] device = sb.ToString().Split(' ');
            return (instance, device);
        }
        Matrix4x4 headPos;
        public override HmdPoseState WaitForPoses()
        {
            if (Disposed)
            {
                return default(HmdPoseState);
            }
            EVRCompositorError compositorError = _compositor.WaitGetPoses(_devicePoses, Array.Empty<TrackedDevicePose_t>());
            TrackedDevicePose_t hmdPose = _devicePoses[OVR.k_unTrackedDeviceIndex_Hmd];
            headPos = posHelp(hmdPose.mDeviceToAbsoluteTracking);

            Matrix4x4 deviceToAbsolute = ToSysMatrix(hmdPose.mDeviceToAbsoluteTracking);
            Matrix4x4.Invert(deviceToAbsolute, out Matrix4x4 absoluteToDevice);

            Matrix4x4 viewLeft = deviceToAbsolute * _headToEyeLeft;
            Matrix4x4 viewRight = deviceToAbsolute *  _headToEyeRight;

            Matrix4x4.Invert(viewLeft, out Matrix4x4 invViewLeft);
            Matrix4x4.Decompose(invViewLeft, out _, out Quaternion leftRotation, out Vector3 leftPosition);

            Matrix4x4.Invert(viewRight, out Matrix4x4 invViewRight);
            Matrix4x4.Decompose(invViewRight, out _, out Quaternion rightRotation, out Vector3 rightPosition);

            return new HmdPoseState(
                _projLeft, _projRight,
                leftPosition, rightPosition,
                leftRotation, rightRotation);
        }

        public bool PollNextEvent(ref VREvent_t pEvent)
        {
            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));
            return _vrSystem.PollNextEvent( ref pEvent, size);
        }

        public override void SubmitFrame()
        {
            if (Disposed)
            {
                return;
            }
            VREvent_t _vrevent = new VREvent_t();
            if (PollNextEvent(ref _vrevent))
            {
                if (_vrevent.eventType == 700)
                {
                    _vrSystem.AcknowledgeQuit_Exiting();
                    Dispose();
                    return;
                }
                if (_vrevent.eventType == 102)
                {
                    UpdateControllers();
                }
            }
            if (_gd.GetOpenGLInfo(out BackendInfoOpenGL glInfo))
            {
                glInfo.FlushAndFinish();
            }
            updateInput();
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

            if (_gd.GetD3D11Info(out BackendInfoD3D11 d3dInfo))
            {
                texT.eColorSpace = EColorSpace.Gamma;
                texT.eType = ETextureType.DirectX;
                texT.handle = d3dInfo.GetTexturePointer(colorTex);
            }
            else if (_gd.GetOpenGLInfo(out BackendInfoOpenGL openglInfo))
            {
                texT.eColorSpace = EColorSpace.Gamma;
                texT.eType = ETextureType.OpenGL;
                texT.handle = (IntPtr)openglInfo.GetTextureName(colorTex);
            }
            else if (_gd.GetVulkanInfo(out BackendInfoVulkan vkInfo))
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

            EVRCompositorError compositorError = EVRCompositorError.None;
            if (_gd.GetOpenGLInfo(out BackendInfoOpenGL glInfo))
            {
                glInfo.ExecuteOnGLThread(() =>
                {
                    compositorError = compositor.Submit(eye, ref texT, ref boundsT, EVRSubmitFlags.Submit_Default);
                });
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
                case TextureSampleCount.Count1: return 1;
                case TextureSampleCount.Count2: return 2;
                case TextureSampleCount.Count4: return 4;
                case TextureSampleCount.Count8: return 8;
                case TextureSampleCount.Count16: return 16;
                case TextureSampleCount.Count32: return 32;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
