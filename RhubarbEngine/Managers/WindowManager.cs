using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.WindowManager;

namespace RhubarbEngine.Managers
{
    public interface IWindowManager : IManager
    {
        Window MainWindow { get; }
        IReadOnlyList<Window> Windows { get; }
        bool MainWindowOpen { get; }

        Window BuildWindow(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540);
    }

    public class WindowManager : IWindowManager
    {
		private IEngine _engine;

		public Window MainWindow { get; private set; }

        private List<Window> _windows  = new();
        public IReadOnlyList<Window> Windows { get { return _windows; } }

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			_windows = new List<Window>();
			this._engine.Logger.Log("Starting Main Window");
			BuildWindow();
			return this;
		}

		public Window BuildWindow(string windowName = "RhubarbVR", int Xpos = 100, int Ypos = 100, int windowWidth = 960, int windowHeight = 540)
		{
			var win = new Window(windowName, Xpos, Ypos, windowWidth, windowHeight);
            _windows.Add(win);
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
