using System;
using RhubarbEngine.Managers;

namespace RhubarbEngine
{
    public class Engine
    {
        public bool verbose;

        public WorldManager worldManager;

        public Managers.NetManager netManager;

        public PlatformInfoManager platformInfo;

        public Managers.WindowManager windowManager;

        public UnitLogs logger;

        public string ip;

        public EngineInitializer engineInitializer;
        public void initialize(string[] _args, bool _verbose = false, bool _Rendering = true)
        {
            verbose = _verbose;
            logger = new UnitLogs(this);
            engineInitializer = new EngineInitializer(this);
            logger.Log("Loading Arguments:", true);
            engineInitializer.loadArguments(_args);
            engineInitializer.initializeManagers();


        }

        public void startUpdateLoop()
        {
            netManager.addClient(ip);
            while (windowManager.mainWindowOpen)
            {
                Loop(platformInfo.startTime, platformInfo.Frame);
                platformInfo.Frame = DateTime.UtcNow;
                platformInfo.FrameCount++;
            }
        }

        public void Loop(DateTime startTime, DateTime Frame)
        {
            windowManager.Update();
            netManager.Update();
        }

        public void cleanUP()
        {
            logger.cleanUP();
            netManager.cleanup();
        }
    }
}
