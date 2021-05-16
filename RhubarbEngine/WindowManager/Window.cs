using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using RhubarbEngine.VirtualReality;

namespace RhubarbEngine.WindowManager
{
    public class Window
    {
        public Sdl2Window window;

        public Window(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = Xpos,
                Y = Ypos,
                WindowWidth = windowWidth,
                WindowHeight = windowHeight,
                WindowTitle = windowName
            };
            window = VeldridStartup.CreateWindow(ref windowCI);

        }

        public void Update()
        {
            window.PumpEvents();
        }
        public bool windowOpen => window.Exists;
        public (GraphicsDevice gd, Swapchain sc) CreateScAndGD(VRContext vrc, GraphicsBackend backend)
        {
            GraphicsDeviceOptions gdo = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Improved, true, true, true);
            if (backend == GraphicsBackend.Vulkan)
            {
                (string[] instance, string[] device) = vrc.GetRequiredVulkanExtensions();
                VulkanDeviceOptions vdo = new VulkanDeviceOptions(instance, device);
                GraphicsDevice gd = GraphicsDevice.CreateVulkan(gdo, vdo);
                Swapchain sc = gd.ResourceFactory.CreateSwapchain(new SwapchainDescription(
                    VeldridStartup.GetSwapchainSource(window),
                    (uint)window.Width, (uint)window.Height,
                    gdo.SwapchainDepthFormat, gdo.SyncToVerticalBlank, true));
                return (gd, sc);
            }
            else
            {
                GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(window, gdo, backend);
                Swapchain sc = gd.MainSwapchain;
                return (gd, sc);
            }
        }
    }
}
