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
    public interface IEngineInitializer
    {
        void InitializeManagers();
        void LoadArguments(string[] _args);
        bool Initialised { get; }
        IEnumerable<string> Settings { get; }
        string Session { get; }
        bool CreateLocalWorld { get; set; }
    }

    public class BaseEngineInitializer: EngineInitializer<PlatformInfoManager,Managers.WindowManager,InputManager,RenderManager,AudioManager,NetApiManager,WorldManager>
    {
        public BaseEngineInitializer(Engine _engine):base(_engine)
        {
        }
    }


    public class EngineInitializer<TPlatformInfoManager, TWindowManager, TInputManager, TRenderManager , TAudioManager, TNetApiManager, TWorldManager> 
    : IEngineInitializer where TRenderManager : IRenderManager, new() where TInputManager : IInputManager, new() 
        where TPlatformInfoManager : IPlatformInfoManager, new() where TWindowManager : IWindowManager, new()
        where TAudioManager : IAudioManager, new() where TNetApiManager : INetApiManager, new()
         where TWorldManager : IWorldManager, new()
    {
		private readonly Engine _engine;

		public string intphase;

		public bool Initialised = false;

        public bool CreateLocalWorld { get; set; }

		public EngineInitializer(Engine _engine)
		{
			this._engine = _engine;
		}

		public void InitializeManagers()
		{
			//This is to make finding memory problems easier
			//System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;

				_engine.Logger.Log("Starting Managers");

				intphase = "Platform Info Manager";
				_engine.Logger.Log("Starting Platform Info Manager:");
				_engine.platformInfo = new TPlatformInfoManager();
				_engine.PlatformInfo.Initialize(_engine);

				if (_engine.PlatformInfo.Platform != Platform.Android)
				{
					intphase = "Window Manager";
					_engine.Logger.Log("Starting Window Manager:");
					_engine.windowManager = new TWindowManager();
					_engine.WindowManager.Initialize(_engine);
				}
				else
				{

				}

				intphase = "Input Manager";
				_engine.Logger.Log("Starting Input Manager:");
				_engine.inputManager = new TInputManager();
				_engine.InputManager.Initialize(_engine);

				intphase = "Render Manager";
				_engine.Logger.Log("Starting Render Manager:");
				_engine.renderManager = new TRenderManager();
				_engine.RenderManager.Initialize(_engine);


				intphase = "Audio Manager";
				_engine.Logger.Log("Starting Audio Manager:");
				_engine.audioManager = new TAudioManager();
				_engine.AudioManager.Initialize(_engine);

				intphase = "Net Api Manager";
				_engine.Logger.Log("Starting Net Api Manager:");
				_engine.netApiManager = new TNetApiManager();
				if (token != null)
				{
					_engine.NetApiManager.Token = token;
				}
				_engine.NetApiManager.Initialize(_engine);

				intphase = "World Manager";
				_engine.Logger.Log("Starting World Manager:");
				_engine.worldManager = new TWorldManager();
				_engine.WorldManager.Initialize(_engine);

            if (_engine.Audio)
            {
                _engine.AudioManager.Task.Start();
            }
            Initialised = true;
		}

		public string token;

		public string session;

		public IEnumerable<string> settings = Array.Empty<string>();

        public IEnumerable<string> Settings
        {
            get
            {
                return settings;
            }
        }

        bool IEngineInitializer.Initialised
        {
            get
            {
                return Initialised;
            }
        }

        public string Session
        {
            get
            {
                return session;
            }
        }

        public void LoadArguments(string[] _args)
		{
			Parser.Default.ParseArguments<CommandLineOptions>(_args)
				.WithParsed(o =>
				{
					if (o.Verbose)
					{
						_engine.Verbose = true;
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
