using System;
using System.IO;
using RhubarbEngine.Managers;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Settings;
using RhuSettings;
using System.Collections.Generic;
using DiscordRPC;
using System.Threading;

namespace RhubarbEngine
{
    public interface IEngine
    {
        public bool Verbose { get; set; }

        public string DataPath { get; }

        public IWorldManager WorldManager { get; }

        public IInputManager InputManager { get; }

        public IRenderManager RenderManager { get; }

        public MainSettingsObject SettingsObject { get; }

        public IPlatformInfoManager PlatformInfo { get; }

        public IWindowManager WindowManager { get; }

        public INetApiManager NetApiManager { get; }

        public IAudioManager AudioManager { get; }

        public IUnitLogs Logger { get; }

        public IEngineInitializer EngineInitializer { get; }

        public DiscordRpcClient DiscordRpcClient { get; }

        public OutputType OutputType { get; set; }

        public GraphicsBackend Backend { get; set; }
        bool Rendering { get; set; }
        bool Audio { get; set; }
        string LoginToken { get; set; }

        event Action OnEngineStarted;

        void Initialize<TEngineInitializer, TUnitLogs>(string[] _args, bool _verbose = false, bool createLocalWorld = true)
            where TEngineInitializer : IEngineInitializer where TUnitLogs : IUnitLogs;
        void WaitForNextUpdate();
        void WaitForNextUpdates(int update);
    }

    public class Engine: IEngine
    {
        public bool verbose;

        public IWorldManager worldManager;

        public IInputManager inputManager;

        public IRenderManager renderManager;

        public IPlatformInfoManager platformInfo;

        public IWindowManager windowManager;

        public INetApiManager netApiManager;

        public IAudioManager audioManager;

        public IUnitLogs logger;

        public IEngineInitializer engineInitializer;

        public DiscordRpcClient discordRpcClient;

        public MainSettingsObject settingsObject;
        public GraphicsBackend backend = GraphicsBackend.Vulkan;

		public OutputType outputType;
        public string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RhubarbVR");

		public FileStream lockFile;

        public bool Rendering { get; set; } = true;

        public bool Audio { get; set; } = true;

        public event Action OnEngineStarted;

        public string LoginToken { get; set; }

        public void Initialize<TEngineInitializer, TUnitLogs>(string[] _args, bool _verbose = false, bool createLocalWorld = true) where TEngineInitializer: IEngineInitializer where TUnitLogs : IUnitLogs
        {
            try
			{
                BulletSharp.Loader.Start();
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
            logger = (IUnitLogs)Activator.CreateInstance(typeof(TUnitLogs), this);
            engineInitializer = (IEngineInitializer)Activator.CreateInstance(typeof(TEngineInitializer), this);
            engineInitializer.CreateLocalWorld = createLocalWorld;
            logger.Log("Loading Arguments:", true);
            for (var i = 0; i < _args.Length; i++)
            {
                if(i == 0)
                {
                    Logger.Log(_args[i], true);
                }
                else if (_args[i - 1] != "-token")
                {
                    Logger.Log(_args[i], true);
                }
                else
                {
                    LoginToken = _args[i];
                }
            }
            engineInitializer.LoadArguments(_args);
			logger.Log("Data path: " + dataPath);
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
			foreach (var item in engineInitializer.Settings)
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
			engineInitializer = engineInitializer.Initialised ? null : throw new Exception("Engine not Initialized");
            OnEngineStarted?.Invoke();
            while (windowManager.MainWindowOpen||!Rendering)
			{
				Loop(platformInfo.StartTime, platformInfo.Frame);
				platformInfo.Frame = DateTime.UtcNow;
                platformInfo.NextFrame();
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

        public IWorldManager WorldManager
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

        public IRenderManager RenderManager
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

        public IPlatformInfoManager PlatformInfo
        {
            get
            {
                return platformInfo;
            }
        }

        public IWindowManager WindowManager
        {
            get
            {
                return windowManager;
            }
        }

        public INetApiManager NetApiManager
        {
            get
            {
                return netApiManager;
            }
        }

        public IAudioManager AudioManager
        {
            get
            {
                return audioManager;
            }
        }

        public IUnitLogs Logger
        {
            get
            {
                return logger;
            }
        }

        public IEngineInitializer EngineInitializer
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
			var newtime = platformInfo.Elapsed.TotalSeconds;
			Console.WriteLine(mark + " : " + (newtime - lastTimemark).ToString());
			lastTimemark = newtime;
		}

        private readonly ManualResetEvent _waiter = new(false);

        public void Loop(DateTime startTime, DateTime Frame)
		{
            _waiter.Set();
            _waiter.Reset();
            platformInfo.Update();
			discordRpcClient.Invoke();
			windowManager.Update();
			inputManager.Update();
			worldManager.Update(startTime, Frame);
			renderManager.Update();
            audioManager.Update();
		}

        public void WaitForNextUpdates(int update)
        {
            for (var i = 0; i < update; i++)
            {
                WaitForNextUpdate();
            }
        }

        public void WaitForNextUpdate()
        {
            _waiter.WaitOne();
        }

		public void CleanUP()
		{
            logger.Log("Closing Audio Manager");
			audioManager.CleanUp();
            logger.Log("Closing World Manager");
            worldManager.CleanUp();
            Logger.CleanUP();
        }
    }
}
