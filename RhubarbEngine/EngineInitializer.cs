using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using CommandLine;
using RhubarbEngine.PlatformInfo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace RhubarbEngine
{
	public class EngineInitializer
	{
		private readonly Engine _engine;

		public string intphase;

		public bool Initialised = false;

		public EngineInitializer(Engine _engine)
		{
			this._engine = _engine;
		}

		public void InitializeManagers()
		{
			//This is to make finding memory problems easier
			//System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
			try
			{
				_engine.logger.Log("Starting Managers");

				intphase = "Platform Info Manager";
				_engine.logger.Log("Starting Platform Info Manager:");
				_engine.platformInfo = new PlatformInfoManager();
				_engine.platformInfo.Initialize(_engine);

				if (_engine.platformInfo.platform != Platform.Android)
				{
					intphase = "Window Manager";
					_engine.logger.Log("Starting Window Manager:");
					_engine.windowManager = new Managers.WindowManager();
					_engine.windowManager.Initialize(_engine);
				}
				else
				{

				}

				intphase = "Input Manager";
				_engine.logger.Log("Starting Input Manager:");
				_engine.inputManager = new Managers.InputManager();
				_engine.inputManager.Initialize(_engine);

				intphase = "Render Manager";
				_engine.logger.Log("Starting Render Manager:");
				_engine.renderManager = new Managers.RenderManager();
				_engine.renderManager.Initialize(_engine);


				intphase = "Audio Manager";
				_engine.logger.Log("Starting Audio Manager:");
				_engine.audioManager = new Managers.AudioManager();
				_engine.audioManager.Initialize(_engine);

				intphase = "Net Api Manager";
				_engine.logger.Log("Starting Net Api Manager:");
				_engine.netApiManager = new Managers.NetApiManager();
				if (token != null)
				{
					_engine.netApiManager.token = token;
				}
				_engine.netApiManager.Initialize(_engine);

				intphase = "World Manager";
				_engine.logger.Log("Starting World Manager:");
				_engine.worldManager = new WorldManager();
				_engine.worldManager.Initialize(_engine);

				_engine.audioManager.task.Start();
				Initialised = true;
			}
			catch (Exception _e)
			{
				_engine.logger.Log("Failed at " + intphase + " Error: " + _e);
			}

		}

		public string token;

		public string session;

		public IEnumerable<string> settings = Array.Empty<string>();

		public void LoadArguments(string[] _args)
		{
			foreach (var arg in _args)
			{
				_engine.logger.Log(arg, true);
			}
			Parser.Default.ParseArguments<CommandLineOptions>(_args)
				.WithParsed<CommandLineOptions>(o =>
				{
					if (o.Verbose)
					{
						_engine.verbose = true;
					}
					if (o.Datapath != null)
					{
						_engine.dataPath = o.Datapath;
					}
				    _engine.backend = o.GraphicsBackend;
				    _engine.outputType = o.OutputType;
					if (o.Settings != null)
					{
						settings = o.Settings;
					}
					if (o.Token != null)
					{
						token = o.Token;
					}
					if (o.SessionID != null)
					{
						session = o.SessionID;
					}
				});
		}
	}
}
