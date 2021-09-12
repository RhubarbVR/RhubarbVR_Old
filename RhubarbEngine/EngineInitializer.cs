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
				_engine.platformInfo.initialize(_engine);

				if (_engine.platformInfo.platform != Platform.Android)
				{
					intphase = "Window Manager";
					_engine.logger.Log("Starting Window Manager:");
					_engine.windowManager = new Managers.WindowManager();
					_engine.windowManager.initialize(_engine);
				}
				else
				{

				}

				intphase = "Input Manager";
				_engine.logger.Log("Starting Input Manager:");
				_engine.inputManager = new Managers.InputManager();
				_engine.inputManager.initialize(_engine);

				intphase = "Render Manager";
				_engine.logger.Log("Starting Render Manager:");
				_engine.renderManager = new Managers.RenderManager();
				_engine.renderManager.initialize(_engine);


				intphase = "Audio Manager";
				_engine.logger.Log("Starting Audio Manager:");
				_engine.audioManager = new Managers.AudioManager();
				_engine.audioManager.initialize(_engine);

				intphase = "Net Api Manager";
				_engine.logger.Log("Starting Net Api Manager:");
				_engine.netApiManager = new Managers.NetApiManager();
				if (token != null)
				{
					_engine.netApiManager.token = token;
				}
				_engine.netApiManager.initialize(_engine);

				intphase = "World Manager";
				_engine.logger.Log("Starting World Manager:");
				_engine.worldManager = new WorldManager();
				_engine.worldManager.initialize(_engine);

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

		public IEnumerable<string> settings = new string[] { };

		public void LoadArguments(string[] _args)
		{
			foreach (var arg in _args)
			{
				_engine.logger.Log(arg, true);
			}
			Parser.Default.ParseArguments<CommandLineOptions>(_args)
				.WithParsed<CommandLineOptions>(o =>
				{
					if (o.verbose)
					{
						_engine.verbose = true;
					}
					if (o.datapath != null)
					{
						_engine.dataPath = o.datapath;
					}
				    _engine.backend = o.graphicsBackend;
				    _engine.outputType = o.outputType;
					if (o.settings != null)
					{
						settings = o.settings;
					}
					if (o.token != null)
					{
						token = o.token;
					}
					if (o.sessionID != null)
					{
						session = o.sessionID;
					}
				});
		}
	}
}
