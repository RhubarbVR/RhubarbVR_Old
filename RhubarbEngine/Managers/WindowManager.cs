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
		private IEngine _engine;

		public Window MainWindow { get; private set; }

		public List<Window> Windows { get; private set; }

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			Windows = new List<Window>();
			this._engine.Logger.Log("Starting Main Window");
			BuildWindow();
			return this;
		}

		public Window BuildWindow(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
		{
			var win = new Window(windowName, Xpos, Ypos, windowWidth, windowHeight);
			Windows.Add(win);
			if (Windows.Count == 1)
			{
				MainWindow = Windows[0];
			}
			return win;
		}

		public void Update()
		{
			foreach (var window in Windows)
			{
				_engine.InputManager.MainWindows.UpdateFrameInput(window.Update(), window.window);
			}
		}

        public bool MainWindowOpen
        {
            get
            {
                return MainWindow.WindowOpen;
            }
        }
    }
}
