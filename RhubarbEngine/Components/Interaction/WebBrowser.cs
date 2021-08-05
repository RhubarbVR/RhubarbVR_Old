using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using System.Numerics;
using RhubarbEngine.World.Asset;

using Veldrid;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Audio;

using CefSharp;
using CefSharp.OffScreen;
using Veldrid.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CefSharp.Structs;
using RhubarbEngine.Managers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace RhubarbEngine.Components.Interaction
{
    public class CustomMenuHandler : CefSharp.IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {

            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }

    [Category(new string[] { "Interaction" })]
    public class WebBrowser : AssetProvider<RTexture2D>, IRenderObject, KeyboardStealer, IAudioHandler,IAudioSource
    {
        public RenderFrequency renderFrac => renderFrequency.value;

        public bool IsActive => loaded;

        public int ChannelCount => 1;

        byte[] frameInputBuffer = new byte[AudioManager.AudioFrameSizeInBytes];

        public byte[] FrameInputBuffer => frameInputBuffer;

        public Sync<RenderFrequency> renderFrequency;
        public Sync<Vector2u> scale;
        public SyncRef<IinputPlane> imputPlane;
        public Sync<string> path;

        private bool loaded;

        ChromiumWebBrowser browser;
        public override void OnAttach()
        {
            base.OnAttach();
       
        }

        public override void onLoaded()
        {
            base.onLoaded();
            if (!Cef.IsInitialized) // Check before init
            {
                Console.WriteLine("Init Cef");
                var cefSettings = new CefSettings();
                cefSettings.CachePath = engine.dataPath + @"\WebBrowser";
                cefSettings.CefCommandLineArgs.Add("enable-media-stream", "1");
                cefSettings.CefCommandLineArgs.Add("disable-usb-keyboard-detect", "1");
                cefSettings.EnableAudio();
                Cef.Initialize(cefSettings);
            }
            browser = new ChromiumWebBrowser(path.value, null, null, false);
            browser.AudioHandler = this;
            browser.MenuHandler = new CustomMenuHandler();
            
            browser.CreateBrowser();
            
            browser.Size = new System.Drawing.Size { Width = (int)scale.value.x, Height = (int)scale.value.y };            
            loaded = true;
        }


        public override void buildSyncObjs(bool newRefIds)
        {
            renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
            scale = new Sync<Vector2u>(this, newRefIds);
            imputPlane = new SyncRef<IinputPlane>(this, newRefIds);
            scale.value = new Vector2u(600, 600);
            scale.Changed += onScaleChange;
            path = new Sync<string>(this, newRefIds);
            path.Changed += Path_Changed;
            path.value = "https://www.youtube.com/watch?v=Rp6ehxZvvM4";
        }

        private void onScaleChange(IChangeable obj)
        {
            if (!loaded) return;
            browser.Size = new System.Drawing.Size { Width = (int)scale.value.x, Height = (int)scale.value.y };
        }

        private void Path_Changed(IChangeable obj)
        {
            if (!loaded) return;
            browser.LoadUrlAsync(path.value);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            Render();
        }
        TextureView view;
        Texture target;
        public void Render()
        {
            if (!loaded) return;
            startRenderTask();
            updateInpute();
        }

        private Task lastTask;

        public void startRenderTask()
        {
            if (lastTask != null) return;
            lastTask = Task.Run(RenderTask);
        }

        private void RenderTask()
        {
            if (view == null)
            {
                target = (new ImageSharpTexture(browser.ScreenshotOrNull(PopupBlending.Main).ToImageSharpImage<Rgba32>(), false)).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                view = engine.renderManager.gd.ResourceFactory.CreateTextureView(target);
                var e = new RTexture2D(view);
                e.addDisposable(target);
                e.addDisposable(view);
                load(e, true);
            }
            else
            {
                //target.UpdateTexture((new ImageSharpTexture(browser.ScreenshotOrNull(PopupBlending.Main).ToImageSharpImage<Rgba32>(),false)), engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                target.UpdateTextureBmp(browser.ScreenshotOrNull(PopupBlending.Main), engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            }
            lastTask = null;
        }

        public void updateInpute()
        {
            if (imputPlane.target == null) return;
            var imp = imputPlane.target;
            foreach (var item in imp.KeyEvents)
            {
                CefSharp.KeyEvent k = new CefSharp.KeyEvent();
                var lp = (int)item.Key;
                k.FocusOnEditableField = true;
                if (lp >= 83 && lp <= 108)
                {
                    k.WindowsKeyCode = lp + (-83 + 65);
                }
                if (lp >= 109 && lp <= 118)
                {
                    k.WindowsKeyCode = lp + (-109 + 48);
                }
                if (lp >= 67 && lp <= 118)
                {
                    k.WindowsKeyCode = lp + (-67 + 76);
                }
                if (lp >= 10 && lp <= 33)
                {
                    k.WindowsKeyCode = lp + (-10 + 112);
                }
                if (lp == 49)
                {
                    k.WindowsKeyCode = lp + (-49 + 13);
                }
                if (lp == 53)
                {
                    k.WindowsKeyCode = lp + (-53 + 8);
                }
                k.Modifiers = CefEventFlags.None;
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Alt)) > 0f)
                {
                    k.Modifiers |= CefEventFlags.AltDown;
                }
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Control)) > 0f)
                {
                    k.Modifiers |= CefEventFlags.ControlDown;
                }
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Shift)) > 0f)
                {
                    k.Modifiers |= CefEventFlags.ShiftDown;
                }
                k.Type = (item.Down) ? KeyEventType.KeyDown : KeyEventType.KeyUp;
                k.IsSystemKey = false;
                browser.GetBrowser().GetHost().SendKeyEvent(k);
                Console.WriteLine(item.ToString());
            }
            foreach (var item in imp.KeyCharPresses)
            {
                CefSharp.KeyEvent k = new CefSharp.KeyEvent();
                var lp = (int)item;
                k.WindowsKeyCode = lp;
                k.FocusOnEditableField = true;
                k.IsSystemKey = false;
                k.Type = KeyEventType.Char;
                browser.GetBrowser().GetHost().SendKeyEvent(k);
            }

            foreach (var item in imp.MouseEvents)
            {
                var key = item.MouseButton;
                switch (key)
                {
                    case MouseButton.Left:
                        browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Left, !item.Down, 1, CefEventFlags.None);
                        break;
                    case MouseButton.Middle:
                        browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Middle, !item.Down, 1, CefEventFlags.None);
                        break;
                    case MouseButton.Right:
                        browser.GetBrowser().GetHost().SendMouseClickEvent((int)imp.MousePosition.X, (int)imp.MousePosition.Y, MouseButtonType.Right, !item.Down, 1, CefEventFlags.None);
                        break;
                    default:
                        break;
                }
            }

            browser.GetBrowser().GetHost().SendMouseMoveEvent(new CefSharp.MouseEvent((int)imp.MousePosition.X,(int)imp.MousePosition.Y, CefEventFlags.None), !imp.focused);
            browser.GetBrowser().GetHost().SendMouseWheelEvent(0, (int)(imp.WheelDelta * 100), 0, (int)(imp.WheelDelta * 100), CefEventFlags.None);
            browser.GetBrowser().GetHost().SetAudioMuted(false);
            browser.GetBrowser().GetHost().SetFocus(imp.focused);
         
        }
        public bool GetAudioParameters(IWebBrowser chromiumWebBrowser, IBrowser browser, ref AudioParameters parameters)
        {
            parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutMono;
            parameters.FramesPerBuffer = AudioManager.AudioFrameSize;
            parameters.SampleRate = AudioManager.SamplingRate;
            return true;
        }

        public void OnAudioStreamStarted(IWebBrowser chromiumWebBrowser, IBrowser browser, AudioParameters parameters, int channels)
        {
           
        }

        public unsafe void OnAudioStreamPacket(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr data, int noOfFrames, long pts)
        {
            unsafe
            {
                float** channelData = (float**)data.ToPointer();
                int chan = 1;
                int size = AudioManager.AudioFrameSizeInBytes * chan;
                byte[] samples = new byte[size];
                fixed (byte* pDestByte = samples)
                {
                    float* pDest = (float*)pDestByte;

                    for (int i = 0; i < noOfFrames; i++)
                    {
                        for (int c = 0; c < chan; c++)
                        {
                            *pDest++ = channelData[c][i];
                        }
                    }
                }
                frameInputBuffer = samples;
            }
        }

        public void OnAudioStreamStopped(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {

        }

        public void OnAudioStreamError(IWebBrowser chromiumWebBrowser, IBrowser browser, string errorMessage)
        {
            logger.Log("Browser Audio Error" + errorMessage, true);
        }

        public WebBrowser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public WebBrowser()
        {
        }
    }
}
