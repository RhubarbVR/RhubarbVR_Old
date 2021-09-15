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

        public int Width
        {
            get
            {
                return window.Width;
            }
        }

        public int Height
        {
            get
            {
                return window.Height;
            }
        }

        public float AspectRatio
        {
            get
            {
                return (float)window.Width / (float)window.Height;
            }
        }

        public Window(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
		{
			var windowCI = new WindowCreateInfo()
			{
				X = Xpos,
				Y = Ypos,
				WindowWidth = windowWidth,
				WindowHeight = windowHeight,
				WindowTitle = windowName
			};
			window = VeldridStartup.CreateWindow(ref windowCI);
		}

		public InputSnapshot Update()
		{
			var temp = window.PumpEvents();
			return temp;
		}
        public bool WindowOpen
        {
            get
            {
                return window.Exists;
            }
        }

        public (GraphicsDevice gd, Swapchain sc) CreateScAndGD(VRContext vrc, GraphicsBackend backend)
		{
			var gdo = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Improved, true, true, true);
			if (backend == GraphicsBackend.Vulkan)
			{
				(var instance, var device) = vrc.GetRequiredVulkanExtensions();
				var vdo = new VulkanDeviceOptions(instance, device);
				var gd = GraphicsDevice.CreateVulkan(gdo, vdo);
				var sc = gd.ResourceFactory.CreateSwapchain(new SwapchainDescription(
					VeldridStartup.GetSwapchainSource(window),
					(uint)window.Width, (uint)window.Height,
					gdo.SwapchainDepthFormat, gdo.SyncToVerticalBlank, true));
				return (gd, sc);
			}
			else
			{
				var gd = VeldridStartup.CreateGraphicsDevice(window, gdo, backend);
				var sc = gd.MainSwapchain;
				return (gd, sc);
			}
		}
	}
}
