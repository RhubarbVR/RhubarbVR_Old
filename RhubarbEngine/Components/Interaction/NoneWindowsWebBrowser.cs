#if !Windows
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using System.Numerics;
using RhubarbEngine.World.Asset;
using Chromely;
using Chromely.CefGlue.Browser;
using Chromely.CefGlue;
using Veldrid;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Audio;

using Veldrid.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RhubarbEngine.Managers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;
using RhubarbEngine.Render;
using Chromely.Core;
using Chromely.Core.Network;
using Chromely.Core.Configuration;
using Chromely.Core.Infrastructure;
namespace RhubarbEngine.Components.Interaction
{

	[Category(new string[] { "Interaction" })]
	public class WebBrowser : AssetProvider<RTexture2D>, IRenderObject, IKeyboardStealer, IAudioSource, IChromelyContainer, IChromelyCommandTaskRunner, IChromelyConfiguration
    {
        public bool Threaded
        {
            get
            {
                return true;
            }
        }

        public RenderFrequency RenderFrac
        {
            get
            {
                return renderFrequency.Value;
            }
        }

        public bool IsActive { get; private set; }

        public int ChannelCount
        {
            get
            {
                return (int)audioType.Value;
            }
        }

        RollBuffer _frameInputBuffer;

        public byte[] FrameInputBuffer
        {
            get
            {
                return _frameInputBuffer.array;
            }
        }

        public string AppName
        {
            get
            {
                return "RhubarbVR";
            }

            set
            {
            }
        }

        public string StartUrl
        {
            get
            {
                return "https://google.com/";
            }

            set
            {
            }
        }

        public string AppExeLocation
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }

            set
            {
            }
        }

        public string ChromelyVersion
        {
            get
            {
                return "0.0.1";
            }

            set
            {
            }
        }

        public ChromelyPlatform Platform
        {
            get
            {
                return Engine.PlatformInfo.Platform switch
                {
                    PlatformInfo.Platform.UNKNOWN => ChromelyPlatform.NotSupported,
                    PlatformInfo.Platform.Windows => ChromelyPlatform.Windows,
                    PlatformInfo.Platform.OSX => ChromelyPlatform.MacOSX,
                    PlatformInfo.Platform.iOS => ChromelyPlatform.NotSupported,
                    PlatformInfo.Platform.Linux => ChromelyPlatform.Linux,
                    PlatformInfo.Platform.Android => ChromelyPlatform.Linux,
                    PlatformInfo.Platform.Other => ChromelyPlatform.NotSupported,
                    _ => ChromelyPlatform.NotSupported,
                };
            }

            set
            {
            }
        }

        public bool DebuggingMode
        {
            get
            {
                return true;
            }

            set
            {
            }
        }

        public string DevToolsUrl
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IDictionary<string, string> CommandLineArgs
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> CommandLineOptions
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<ControllerAssemblyInfo> ControllerAssemblies
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IDictionary<string, string> CustomSettings
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<ChromelyEventHandler<object>> EventHandlers
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IDictionary<string, object> ExtensionData
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IChromelyJavaScriptExecutor JavaScriptExecutor
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<UrlScheme> UrlSchemes
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public CefDownloadOptions CefDownloadOptions
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IWindowOptions WindowOptions
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Sync<RenderFrequency> renderFrequency;
		public Sync<Vector2u> scale;
		public SyncRef<IInputPlane> imputPlane;
		public Sync<string> path;
		public Driver<string> title;
		public Sync<bool> globalAudio;
		public Sync<bool> noKeyboard;
		public Sync<AudioType> audioType;

		CefGlueBrowser _browser;

		public override void Dispose()
		{

            Load(null, true);
            _browser?.Dispose();
            base.Dispose();
		}

		public override void OnAttach()
		{
			base.OnAttach();

		}
		public override void OnLoaded()
		{
			base.OnLoaded();
            _frameInputBuffer = new RollBuffer(Engine.AudioManager.AudioFrameSizeInBytes * ChannelCount);
            _browser = new CefGlueBrowser(this, this,this,this,new Xilium.CefGlue.Wrapper.CefMessageRouterBrowserSide(new Xilium.CefGlue.Wrapper.CefMessageRouterConfig()),new Xilium.CefGlue.CefBrowserSettings());
            _browser.Create(Xilium.CefGlue.CefWindowInfo.Create());
		}


		private void TakeKeyboard()
		{
			if (noKeyboard.Value)
            {
                return;
            }

            Input.Keyboard = this;
			if (imputPlane.Target != null)
			{
				imputPlane.Target.StopMouse = true;
			}
		}
		private void LoseKeyboard()
		{
			if (Input.Keyboard == this)
			{
				Input.Keyboard = null;
			}
			if (imputPlane.Target != null)
			{
				imputPlane.Target.StopMouse = false;
			}
		}
		

		public override void BuildSyncObjs(bool newRefIds)
		{
			renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
			scale = new Sync<Vector2u>(this, newRefIds);
			imputPlane = new SyncRef<IInputPlane>(this, newRefIds);
			scale.Value = new Vector2u(600, 600);
			scale.Changed += OnScaleChange;
			path = new Sync<string>(this, newRefIds);
			globalAudio = new Sync<bool>(this, newRefIds);
            audioType = new Sync<AudioType>(this, newRefIds)
            {
                Value = AudioType.LayoutMono
            };
            audioType.Changed += AudioType_Changed;
			globalAudio.Changed += GlobalAudio_Changed;
			path.Value = "https://www.youtube.com/watch?v=Rp6ehxZvvM4";
			path.Changed += Path_Changed;
			//path.value = "https://google.com/";
			//path.value = "https://g.co/arts/xoCTBcR4S3MD8QPE6";
			//path.value = "https://www.vsynctester.com/testing/mouse.html";
			title = new Driver<string>(this, newRefIds);
			noKeyboard = new Sync<bool>(this, newRefIds);

		}

		private void AudioType_Changed(IChangeable obj)
		{
			IsActive = false;
			try
			{
				_frameInputBuffer.Push(new byte[Engine.AudioManager.AudioFrameSizeInBytes * ChannelCount]);
				_browser.Dispose();
			}
			catch { }
			OnLoaded();
		}

		private void GlobalAudio_Changed(IChangeable obj)
		{
			if (_browser == null)
            {
                return;
            }

            if (globalAudio.Value)
			{
				_frameInputBuffer.Push(new byte[Engine.AudioManager.AudioFrameSizeInBytes * ChannelCount]);
				//_browser.AudioHandler = null;
			}
			else
			{
				//_browser.AudioHandler = this;
			}
		}

		private void OnScaleChange(IChangeable obj)
		{
			if (!IsActive)
            {
                return;
            }
            //_browser. = new Vector2 { Width = (int)scale.Value.x, Height = (int)scale.Value.y };
		}

		private void Path_Changed(IChangeable obj)
		{
			if (!IsActive)
            {
                return;
            }

   //         if (_browser.IsBrowserInitialized)
			//{
			//	if (path.Value != _browser.Address)
   //             {
   //                 _browser.LoadUrlAsync(path.Value);
   //             }
   //         }
			//else
			//{
			//	if (path.Value != _browser.Address)
   //             {
   //                 _updateUrl = true;
   //             }
   //         }
		}

        //readonly TextureView _view;
        //readonly UpdateDatingTexture2D _target;

		public void Render()
		{
			if (!IsActive)
            {
                return;
            }

            StartRenderTask();
			UpdateInpute();
		}
		public override void LoadListObject()
		{
			try
			{
				World.updateLists.trenderObject.SafeAdd(this);
			}
			catch { }
		}

		public override void RemoveListObject()
		{
			try
			{
				World.updateLists.trenderObject.Remove(this);
			}
			catch { }
		}

		private Task _lastTask;

		public void StartRenderTask()
		{
			if (_lastTask != null)
			{
				if ((!_lastTask.IsFaulted) && !_lastTask.IsCompleted)
				{
					return;
				}
			}
			_lastTask = Task.Run(RenderTask);
		}

		private void RenderTask()
		{
			try
			{
				//if (_view == null)
				//{
				//	var thing = ScreenshotOrNull(_browser, PopupBlending.Main);
				//	if (thing == null)
    //                {
    //                    return;
    //                }

    //                _target = new UpdateDatingTexture2D();
				//	_view = _target.InitializeView(((RenderHandler)_browser.RenderHandler).BitmapBuffer.CreateDeviceTexture(Engine.renderManager.gd, Engine.renderManager.gd.ResourceFactory), Engine.renderManager.gd);
				//	var e = new RTexture2D(_view);
				//	e.AddDisposable(_target);
				//	e.AddDisposable(_view);
				//	Load(e, true);
				//}
				//else
				//{
				//	// This still has memmory problems I believe it is a problem with the staging texture not geting disposed properly somewhere
				//	((RenderHandler)_browser.RenderHandler).renderEvent.WaitOne();
				//	((RenderHandler)_browser.RenderHandler).renderEvent.Reset();
				//	_target.UpdateBitmap(((RenderHandler)_browser.RenderHandler).BitmapBuffer);
				//}
			}
			catch (Exception e)
			{
				Console.WriteLine("WebBrowser Render Error" + e.ToString());
			}
			_lastTask = null;
		}

		public void UpdateInpute()
		{
			if (imputPlane.Target == null)
            {
                return;
            }

            //var imp = imputPlane.Target;
			//foreach (var item in imp.KeyEvents)
			//{
			//	var k = new CefSharp.KeyEvent();
			//	var lp = (int)item.Key;
			//	k.FocusOnEditableField = true;
   //             if (lp is not >= 83 or not <= 108)
   //             {
   //                 if (lp is >= 109 and <= 118)
   //                 {
   //                     k.WindowsKeyCode = lp + -109 + 48;
   //                 }
   //                 else if (lp is >= 67 and <= 118)
   //                 {
   //                     k.WindowsKeyCode = lp + -67 + 76;
   //                 }
   //                 else if (lp is >= 10 and <= 33)
   //                 {
   //                     k.WindowsKeyCode = lp + -10 + 112;
   //                 }
   //                 else if (lp == 49)
   //                 {
   //                     k.WindowsKeyCode = lp + -49 + 13;
   //                 }
   //                 else if (lp == 53)
   //                 {
   //                     k.WindowsKeyCode = lp + -53 + 8;
   //                 }
   //             }
   //             else
   //             {
   //                 k.WindowsKeyCode = lp + -83 + 65;
   //             }
   //             k.Modifiers = CefEventFlags.None;
			//	if ((((int)item.Modifiers) & ((int)ModifierKeys.Alt)) > 0f)
			//	{
			//		k.Modifiers |= CefEventFlags.AltDown;
			//	}
			//	if ((((int)item.Modifiers) & ((int)ModifierKeys.Control)) > 0f)
			//	{
			//		k.Modifiers |= CefEventFlags.ControlDown;
			//	}
			//	if ((((int)item.Modifiers) & ((int)ModifierKeys.Shift)) > 0f)
			//	{
			//		k.Modifiers |= CefEventFlags.ShiftDown;
			//	}
			//	k.Type = item.Down ? KeyEventType.KeyDown : KeyEventType.KeyUp;
			//	k.IsSystemKey = false;
			//	_browser.GetBrowser().GetHost().SendKeyEvent(k);
			//}
			//foreach (var item in imp.KeyCharPresses)
			//{
			//	var k = new CefSharp.KeyEvent();
			//	var lp = (int)item;
			//	k.WindowsKeyCode = lp;
			//	k.FocusOnEditableField = true;
			//	k.IsSystemKey = false;
			//	k.Type = KeyEventType.Char;
			//	_browser.GetBrowser().GetHost().SendKeyEvent(k);
			//}

			//foreach (var item in imp.MouseEvents)
			//{
			//	var key = item.MouseButton;
			//	switch (key)
			//	{
			//		case MouseButton.Left:
			//			_browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Left, !item.Down, 1, CefEventFlags.None);
			//			break;
			//		case MouseButton.Middle:
			//			_browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Middle, !item.Down, 1, CefEventFlags.None);
			//			break;
			//		case MouseButton.Right:
			//			_browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Right, !item.Down, 1, CefEventFlags.None);
			//			break;
			//		default:
			//			break;
			//	}
			//}

			//_browser.GetBrowser().GetHost().SendMouseMoveEvent(new CefSharp.MouseEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, CefEventFlags.None), !imp.Focused);
			//_browser.GetBrowser().GetHost().SendMouseWheelEvent(0, (int)(imp.WheelDelta * 100), 0, (int)(imp.WheelDelta * 100), CefEventFlags.None);
			//_browser.GetBrowser().GetHost().SetAudioMuted(false);
			//_browser.GetBrowser().GetHost().SetFocus(imp.Focused);

		}

        public bool IsRegistered(Type service, string key)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public string[] GetKeys(Type service)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type service, string key, Type implementation)
        {
            throw new NotImplementedException();
        }

        public void RegisterByTypeSingleton(Type service, Type implementation)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TImplementation>(string key)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton<TService, TImplementation>(string key) where TImplementation : TService
        {
            throw new NotImplementedException();
        }

        public void RegisterByTypeSingleton<TService, TImplementation>() where TImplementation : TService
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type service, string key, object instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TService>(string key, TService instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterPerRequest(Type service, string key, Type implementation)
        {
            throw new NotImplementedException();
        }

        public void RegisterPerRequest<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public void RegisterPerRequest<TService, TImplementation>(string key) where TImplementation : TService
        {
            throw new NotImplementedException();
        }

        public bool UnregisterHandler(Type service, string key)
        {
            throw new NotImplementedException();
        }

        public bool UnregisterHandler<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type service, string key)
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object[] GetAllInstances(Type service)
        {
            throw new NotImplementedException();
        }

        public TService[] GetAllInstances<TService>()
        {
            throw new NotImplementedException();
        }

        public void Run(string url)
        {
            throw new NotImplementedException();
        }

        public void RunAsync(string url)
        {
            throw new NotImplementedException();
        }

        public WebBrowser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public WebBrowser()
		{
		}
	}
}
#endif