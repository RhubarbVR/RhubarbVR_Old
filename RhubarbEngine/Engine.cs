using System;
using System.IO;
using RhubarbEngine.Managers;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Settings;
using RhuSettings;
using System.Collections.Generic;

namespace RhubarbEngine
{
    public class Engine
    {
        public bool verbose;

        public WorldManager worldManager;

        public InputManager inputManager;

        public RenderManager renderManager;

        public MainSettingsObject settingsObject;

        public PlatformInfoManager platformInfo;

        public GraphicsBackend backend = GraphicsBackend.Vulkan;

        public OutputType outputType;

        public Managers.WindowManager windowManager;

        public Managers.NetApiManager netApiManager;

        public UnitLogs logger;

        public string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\RhubarbVR";

        public FileStream lockFile;

        public EngineInitializer engineInitializer;

        public void initialize(string[] _args, bool _verbose = false, bool _Rendering = true)
        {
            verbose = _verbose;
            logger = new UnitLogs(this);
            Logger.init(this);
            engineInitializer = new EngineInitializer(this);
            logger.Log("Loading Arguments:", true);
            engineInitializer.loadArguments(_args);
            logger.Log("Datapath: "+ dataPath);
            //Build DataFolder
            if (!Directory.Exists(dataPath))
            {
                logger.Log("Created data path folder");
                Directory.CreateDirectory(dataPath);
            }
            try
            {
                lockFile = new FileStream(dataPath + "\\locker.lock", FileMode.OpenOrCreate);
            }
            catch
            {
                logger.Log("Another instance is running at data path " + dataPath, true);
                throw new Exception("Another instance is running at data path ");
            }
            List<DataList> lists = new List<DataList>();
            if (File.Exists("settings.json"))
            {
                string text = File.ReadAllText("settings.json");
                DataList liet = SettingsManager.getDataFromJson(text);
                lists.Add(liet);
            }
                foreach (string item in engineInitializer.settings)
                {
                    string text;
                    if (File.Exists(item))
                    {
                        text = File.ReadAllText(item);
                    }
                    else
                    {
                        text = item;
                    }
                    try
                    {
                        DataList liet = SettingsManager.getDataFromJson(text);
                        lists.Add(liet);
                    }
                    catch (Exception e)
                    {
                        logger.Log("Error loading settings ERROR:" + e.ToString(), true);
                    }
                }
            if (lists.Count == 0)
            {
                settingsObject = new MainSettingsObject();
            }
            else
            {
                settingsObject = SettingsManager.loadSettingsObject<MainSettingsObject>(lists.ToArray());
            }
            engineInitializer.initializeManagers();
        }

        public void startUpdateLoop()
        {
            if (engineInitializer.Initialised)
            {
                engineInitializer = null;
            }
            else
            {
                throw new Exception("Engine not Initialised");
            }
            while (windowManager.mainWindowOpen)
            {
                Loop(platformInfo.startTime, platformInfo.Frame);
                platformInfo.Frame = DateTime.UtcNow;
                platformInfo.FrameCount++;
            }
        }

        public double lastTimemark;
        //For performance testing
        public void timeMark(string mark)
        {
            double newtime = platformInfo.sw.Elapsed.TotalSeconds;
            Console.WriteLine(mark + " : " + (newtime - lastTimemark).ToString());
            lastTimemark = newtime;
        }

        public void Loop(DateTime startTime, DateTime Frame)
        {
            inputManager.Update();
            windowManager.Update();
            worldManager.Update(startTime, Frame);
            platformInfo.Update();
            renderManager.Update();
        }

        public void cleanUP()
        {
            worldManager.CleanUp();
            logger.cleanUP();
            netApiManager.Close();
        }
    }
}
