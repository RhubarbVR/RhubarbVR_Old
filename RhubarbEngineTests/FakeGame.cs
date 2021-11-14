using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using RhubarbEngine.WindowManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace RhubarbEngine
{
    public class NullWindowManager : IWindowManager
    {
        public Window MainWindow
        {
            get
            {
                return null;
            }
        }
        private List<Window> _windows;
        public IReadOnlyList<Window> Windows
        {
            get
            {
                return _windows;
            }
        }

        public bool MainWindowOpen
        {
            get
            {
                return true;
            }
        }

        public Window BuildWindow(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
        {
            return null;
        }

        public IManager Initialize(IEngine engine)
        {
            _windows = new List<Window>();
            engine.Logger.Log("Starting Windows");
            return this;
        }

        public void Update()
        {
        }
    }

    public static class RhubarbInstanceCheck
    {
        public static bool InstanceCheck;
        public static Engine engine;
    }


    public abstract class FakeGame
    {
        public Engine engine = new();

        public World.World testWorld;

        private readonly ManualResetEvent _waiter = new(false);

        public void WaitForEngineStart()
        {
            _waiter.WaitOne();
        }

        public FakeGame(bool autostart= true)
        {
            if (autostart)
            {
                StartRhubarbEngine();
            }
        }

        public void StartRhubarbEngine(string dataPathAdd="")
        {
            if (RhubarbInstanceCheck.InstanceCheck)
            {
                engine = RhubarbInstanceCheck.engine;
                _waiter.Set();
                return;
            }
            try
            {
                RhubarbInstanceCheck.InstanceCheck = true;
                engine.dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", $"{DateTime.Now.ToString().Replace("/", "-").Replace(":", "_")}{dataPathAdd}");
                engine.Initialize<EngineInitializer<PlatformInfoManager, NullWindowManager, InputManager,RenderManager,AudioManager,NetApiManager,WorldManager>,UnitLogs>(Array.Empty<string>(), true, false);
                engine.OnEngineStarted += Engine_OnEngineStarted;
                Task.Run(Start);
                RhubarbInstanceCheck.engine = engine;
            }
            catch (Exception e)
            {
                Assert.Fail("Failed to start"+e.ToString());
                engine.Logger.Log(e.ToString(), true);
            }
        }

        private void Engine_OnEngineStarted()
        {
            _waiter.Set();
        }

        public void Start()
        {
            engine.StartUpdateLoop();
        }

        public void NewTestWorld(string name = "The Test World")
        {
            WaitForEngineStart();
            if(testWorld is not null)
            {
                testWorld.Dispose();
                testWorld = null;
            }
            testWorld = engine.worldManager.CreateNewWorld(name);
        }

    }
}
