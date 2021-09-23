using System;
using System.IO;
using RhubarbEngine.Managers;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Settings;
using RhuSettings;
using System.Collections.Generic;
using DiscordRPC;

namespace RhubarbEngine
{
    public interface IEngine
    {
        public bool Verbose { get; set; }

        public string DataPath { get; }

        public WorldManager WorldManager { get; }

        public IInputManager InputManager { get; }

        public RenderManager RenderManager { get; }

        public MainSettingsObject SettingsObject { get; }

        public PlatformInfoManager PlatformInfo { get; }

        public Managers.WindowManager WindowManager { get; }

        public Managers.NetApiManager NetApiManager { get; }

        public AudioManager AudioManager { get; }

        public UnitLogs Logger { get; }

        public EngineInitializer EngineInitializer { get; }

        public DiscordRpcClient DiscordRpcClient { get; }

        public OutputType OutputType { get; set; }

        public GraphicsBackend Backend { get; set; }

    }

    public class Engine: IEngine
    {
        public bool verbose;

        public WorldManager worldManager;

        public IInputManager inputManager;

        public RenderManager renderManager;

        public PlatformInfoManager platformInfo;

        public Managers.WindowManager windowManager;

        public Managers.NetApiManager netApiManager;

        public AudioManager audioManager;

        public UnitLogs logger;

        public EngineInitializer engineInitializer;

        public DiscordRpcClient discordRpcClient;

        public MainSettingsObject settingsObject;
        public GraphicsBackend backend = GraphicsBackend.Vulkan;

		public OutputType outputType;
        public string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RhubarbVR");

		public FileStream lockFile;

#pragma warning disable IDE0060 // Remove unused parameter
        public void Initialize(string[] _args, bool _verbose = false, bool _Rendering = true)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			try
			{
				discordRpcClient = new DiscordRpcClient("678074691738402839");
				//Subscribe to events
				discordRpcClient.RegisterUriScheme("740251");

				discordRpcClient.OnReady += (sender, e) =>
				{
					discordRpcClient.SetPresence(new RichPresence()
					{
						Details = "The Engine",
						State = " starting..",
						Assets = new Assets()
						{
							LargeImageKey = "rhubarbvr",
							LargeImageText = "Faolan Says HI",
							SmallImageKey = "rhubarbvr2"
						}
					});
				};

				discordRpcClient.OnPresenceUpdate += (sender, e) =>
				{

				};

				discordRpcClient.OnJoinRequested += (sender, e) =>
				{

				};

				discordRpcClient.OnJoin += (sender, e) =>
				{

				};
				discordRpcClient.Initialize();

			}
			catch { }

            verbose = _verbose;
			logger = new UnitLogs(this);
			engineInitializer = new EngineInitializer(this);
			logger.Log("Loading Arguments:", true);
			engineInitializer.LoadArguments(_args);
			logger.Log("Datapath: " + dataPath);
			//Build DataFolder
			if (!Directory.Exists(dataPath))
			{
				logger.Log("Created data path folder");
				Directory.CreateDirectory(dataPath);
			}
			try
			{
				lockFile = new FileStream(Path.Combine(dataPath, "locker.lock"), FileMode.OpenOrCreate);
			}
			catch
			{
				logger.Log("Another instance is running at data path " + dataPath, true);
				throw new Exception("Another instance is running at data path ");
			}
			var lists = new List<DataList>();
			if (File.Exists("settings.json"))
			{
				var text = File.ReadAllText("settings.json");
				var liet = SettingsManager.getDataFromJson(text);
				lists.Add(liet);
			}
			foreach (var item in engineInitializer.settings)
			{
				var text = File.Exists(item) ? File.ReadAllText(item) : item;
                try
                {
					var liet = SettingsManager.getDataFromJson(text);
					lists.Add(liet);
				}
				catch (Exception e)
				{
					logger.Log("Error loading settings ERROR:" + e.ToString(), true);
				}
			}
			settingsObject = lists.Count == 0 ? new MainSettingsObject() : SettingsManager.loadSettingsObject<MainSettingsObject>(lists.ToArray());
            engineInitializer.InitializeManagers();
		}

		public void StartUpdateLoop()
		{
			engineInitializer = engineInitializer.Initialised ? null : throw new Exception("Engine not Initialised");
            while (windowManager.MainWindowOpen)
			{
				Loop(platformInfo.startTime, platformInfo.Frame);
				platformInfo.Frame = DateTime.UtcNow;
				platformInfo.FrameCount++;
			}
		}

		public double lastTimemark;

        public bool Verbose
        {
            get
            {
                return verbose;
            }

            set
            {
                verbose = value;
            }
        }

        public WorldManager WorldManager
        {
            get
            {
                return worldManager;
            }
        }

        public IInputManager InputManager
        {
            get
            {
                return inputManager;
            }
        }

        public RenderManager RenderManager
        {
            get
            {
                return renderManager;
            }
        }

        public MainSettingsObject SettingsObject
        {
            get
            {
                return settingsObject;
            }
        }

        public PlatformInfoManager PlatformInfo
        {
            get
            {
                return platformInfo;
            }
        }

        public Managers.WindowManager WindowManager
        {
            get
            {
                return windowManager;
            }
        }

        public NetApiManager NetApiManager
        {
            get
            {
                return netApiManager;
            }
        }

        public AudioManager AudioManager
        {
            get
            {
                return audioManager;
            }
        }

        public UnitLogs Logger
        {
            get
            {
                return logger;
            }
        }

        public EngineInitializer EngineInitializer
        {
            get
            {
                return engineInitializer;
            }
        }

        public DiscordRpcClient DiscordRpcClient
        {
            get
            {
                return discordRpcClient;
            }
        }

        public string DataPath
        {
            get
            {
                return dataPath;
            }
        }

        public OutputType OutputType
        {
            get
            {
                return outputType;
            }

            set
            {
                outputType = value;
            }
        }

        public GraphicsBackend Backend
        {
            get
            {
                return backend;
            }

            set
            {
                backend = value;
            }
        }

        //For performance testing
        public void TimeMark(string mark)
		{
			var newtime = platformInfo.sw.Elapsed.TotalSeconds;
			Console.WriteLine(mark + " : " + (newtime - lastTimemark).ToString());
			lastTimemark = newtime;
		}

		public void Loop(DateTime startTime, DateTime Frame)
		{
			platformInfo.Update();
			discordRpcClient.Invoke();
			windowManager.Update();
			inputManager.Update();
			worldManager.Update(startTime, Frame);
			renderManager.Update().Wait();
		}

		public void CleanUP()
		{
			audioManager.UnloadAll();
			worldManager.CleanUp();
			logger.CleanUP();
		}
	}
}
