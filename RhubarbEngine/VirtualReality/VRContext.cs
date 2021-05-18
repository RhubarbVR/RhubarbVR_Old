﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.VirtualReality.Oculus;
using RhubarbEngine.VirtualReality.OpenVR;
using Veldrid;

namespace RhubarbEngine.VirtualReality
{
    public abstract class VRContext : IDisposable
    {
        internal VRContext() { }

        public abstract void Initialize(GraphicsDevice gd);

        public abstract string DeviceName { get; }

        public bool Disposed;

        public abstract Framebuffer LeftEyeFramebuffer { get; }
        public abstract Framebuffer RightEyeFramebuffer { get; }

        public abstract HmdPoseState WaitForPoses();
        public abstract void SubmitFrame();
        public abstract void RenderMirrorTexture(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source);

        public abstract (string[] instance, string[] device) GetRequiredVulkanExtensions();

        public abstract void Dispose();

        public static VRContext CreateScreen() => CreateScreen(default);
        public static VRContext CreateScreen(VRContextOptions options) => new ScreenContext(options);

        public static VRContext CreateOculus() => CreateOculus(default);
        public static VRContext CreateOculus(VRContextOptions options) => new OculusContext(options);
        public static bool IsOculusSupported() => OculusContext.IsSupported();

        public static VRContext CreateOpenVR() => CreateOpenVR(default);
        public static VRContext CreateOpenVR(VRContextOptions options) => new OpenVRContext(options);
        public static bool IsOpenVRSupported() => OpenVRContext.IsSupported();
    }
}
