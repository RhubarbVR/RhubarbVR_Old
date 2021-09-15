using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Veldrid;
using RhubarbEngine.VirtualReality;

namespace RhubarbEngine
{
	public class CommandLineOptions
	{

		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

		[Option('d', "datapath", Required = false, HelpText = "Set Data Path.")]
		public string Datapath { get; set; }

		[Option('s', "settings", Required = false, HelpText = "Settings")]
		public IEnumerable<string> Settings { get; set; }

		[Option('g', "graphicsbackend", Required = false, HelpText = "Change backend to Direct3D11,Vulkan,OpenGL,Metal,OpenGLES")]
		public GraphicsBackend GraphicsBackend { get; set; } = GraphicsBackend.Vulkan;

		[Option('o', "outputdevice", Required = false, HelpText = "Change output device to Auto,Screen,SteamVR,OculusVR")]
		public OutputType OutputType { get; set; }

		[Option('t', "token", Required = false, HelpText = "Login Token")]
		public string Token { get; set; }

		[Option('j', "joinsession", Required = false, HelpText = "joinsessionID")]
		public string SessionID { get; set; }
	}
}
