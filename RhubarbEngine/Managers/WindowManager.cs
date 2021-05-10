using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.WindowManager;

namespace RhubarbEngine.Managers
{
    public class WindowManager : IManager
    {
        private Engine engine;

        public Window mainWindow { get; private set; }

        public List<Window> windows { get; private set; }

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            windows = new List<Window>();
            engine.logger.Log("Starting Main Window");
            BuildWindow();
            return this;
        }

        public Window BuildWindow(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
        {
            Window win = new Window(windowName, Xpos, Ypos, windowWidth, windowHeight);
            windows.Add(win);
            if (windows.Count == 1)
            {
                mainWindow = windows[0];
            }
            return win;
        }

        public void Update()
        {
            foreach (var window in windows)
            {
                window.Update();
            }
        }

        public bool mainWindowOpen => mainWindow.windowOpen;
    }
}
