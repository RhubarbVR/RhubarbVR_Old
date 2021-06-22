using System;
using System.IO;
using RhubarbEngine.Managers;
using Veldrid;
using RhubarbEngine.VirtualReality;

namespace RhubarbEngine
{
    public class Engine
    {
        public bool verbose;

        public WorldManager worldManager;

        public InputManager inputManager;

        public RenderManager renderManager;

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
            renderManager.Update();
            windowManager.Update();
            worldManager.Update(startTime, Frame);
            platformInfo.Update();
        }

        public void cleanUP()
        {
            worldManager.CleanUp();
            logger.cleanUP();
        }
    }
}
