using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using CefSharp.Enums;
using Point = System.Drawing.Point;
using Range = CefSharp.Structs.Range;
using Size = System.Drawing.Size;
using System.Drawing;

namespace RhubarbEngine.Components.Interaction
{

    /// <summary>
    /// Default implementation of <see cref="IRenderHandler"/>, this class handles Offscreen Rendering (OSR).
    /// Upstream documentation at http://magpcss.org/ceforum/apidocs3/projects/(default)/CefRenderHandler.html
    /// </summary>
    public class RenderHandler : IRenderHandler
    {
        private ChromiumWebBrowser browser;

        private Size popupSize;
        private Point popupPosition;

        public event Action<TextInputMode> keyboard;

        /// <summary>
        /// Need a lock because the caller may be asking for the bitmap
        /// while Chromium async rendering has returned on another thread.
        /// </summary>
        public readonly object BitmapLock = new object();

        /// <summary>
        /// Gets or sets a value indicating whether the popup is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if popup is opened; otherwise, <c>false</c>.
        /// </value>
        public bool PopupOpen { get; protected set; }

        /// <summary>
        /// Contains the last bitmap buffer. Direct access
        /// to the underlying buffer - there is no locking when trying
        /// to access directly, use <see cref="BitmapBuffer.BitmapLock" /> where appropriate.
        /// </summary>
        /// <value>The bitmap.</value>
        public BitmapBuffer BitmapBuffer { get; private set; }

        /// <summary>
        /// The popup Bitmap.
        /// </summary>
        public BitmapBuffer PopupBuffer { get; private set; }

        /// <summary>
        /// Gets the size of the popup.
        /// </summary>
        public Size PopupSize
        {
            get { return popupSize; }
        }

        /// <summary>
        /// Gets the popup position.
        /// </summary>
        public Point PopupPosition
        {
            get { return popupPosition; }
        }

        /// <summary>
        /// Create a new instance of DefaultRenderHadler
        /// </summary>
        /// <param name="browser">reference to the ChromiumWebBrowser</param>
        public RenderHandler(ChromiumWebBrowser browser)
        {
            this.browser = browser;

            popupPosition = new Point();
            popupSize = new Size();

            BitmapBuffer = new BitmapBuffer(BitmapLock);
            PopupBuffer = new BitmapBuffer(BitmapLock);
        }

        /// <summary>
        /// Dispose of this instance.
        /// </summary>
        public void Dispose()
        {
            browser = null;
            BitmapBuffer = null;
            PopupBuffer = null;
        }

        /// <summary>
        /// Called to allow the client to return a ScreenInfo object with appropriate values.
        /// If null is returned then the rectangle from GetViewRect will be used.
        /// If the rectangle is still empty or invalid popups may not be drawn correctly. 
        /// </summary>
        /// <returns>Return null if no screenInfo structure is provided.</returns>	
        public virtual ScreenInfo? GetScreenInfo()
        {
            var screenInfo = new ScreenInfo { DeviceScaleFactor = 1.0F };

            return screenInfo;
        }

        /// <summary>
        /// Called to retrieve the view rectangle which is relative to screen coordinates.
        /// This method must always provide a non-empty rectangle.
        /// </summary>
        /// <returns>Return a ViewRect strict containing the rectangle.</returns>
        public virtual Rect GetViewRect()
        {
            //TODO: See if this can be refactored and remove browser reference
            var size = browser.Size;

            var viewRect = new Rect(0, 0, size.Width, size.Height);

            return viewRect;
        }

        /// <summary>
        /// Called to retrieve the translation from view coordinates to actual screen coordinates. 
        /// </summary>
        /// <param name="viewX">x</param>
        /// <param name="viewY">y</param>
        /// <param name="screenX">screen x</param>
        /// <param name="screenY">screen y</param>
        /// <returns>Return true if the screen coordinates were provided.</returns>
        public virtual bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
        {
            screenX = viewX;
            screenY = viewY;

            return false;
        }

        /// <summary>
        /// Called when an element has been rendered to the shared texture handle.
        /// This method is only called when <see cref="IWindowInfo.SharedTextureEnabled"/> is set to true
        /// </summary>
        /// <param name="type">indicates whether the element is the view or the popup widget.</param>
        /// <param name="dirtyRect">contains the set of rectangles in pixel coordinates that need to be repainted</param>
        /// <param name="sharedHandle">is the handle for a D3D11 Texture2D that can be accessed via ID3D11Device using the OpenSharedResource method.</param>
        public virtual void OnAcceleratedPaint(PaintElementType type, Rect dirtyRect, IntPtr sharedHandle)
        {
            //NOT USED
        }

        /// <summary>
        /// Called when an element should be painted. Pixel values passed to this method are scaled relative to view coordinates based on the
        /// value of <see cref="ScreenInfo.DeviceScaleFactor"/> returned from <see cref="GetScreenInfo"/>.
        /// This method is only called when <see cref="IWindowInfo.SharedTextureEnabled"/> is set to false.
        /// Called on the CEF UI Thread
        /// </summary>
        /// <param name="type">indicates whether the element is the view or the popup widget.</param>
        /// <param name="dirtyRect">contains the set of rectangles in pixel coordinates that need to be repainted</param>
        /// <param name="buffer">The bitmap will be will be  width * height *4 bytes in size and represents a BGRA image with an upper-left origin</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public virtual void OnPaint(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            var isPopup = type == PaintElementType.Popup;

            var bitmapBuffer = isPopup ? PopupBuffer : BitmapBuffer;

            bitmapBuffer.UpdateBuffer(width, height, buffer, dirtyRect);
        }

        /// <summary>
        /// Called when the browser's cursor has changed.
        /// </summary>
        /// <param name="cursor">If type is Custom then customCursorInfo will be populated with the custom cursor information</param>
        /// <param name="type">cursor type</param>
        /// <param name="customCursorInfo">custom cursor Information</param>
        public virtual void OnCursorChange(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
        {
            cursorChange?.Invoke(type);
        }

        public event Action<CursorType> cursorChange;

        /// <summary>
        /// Called when the user starts dragging content in the web view. Contextual information about the dragged content is
        /// supplied by dragData. OS APIs that run a system message loop may be used within the StartDragging call.
        /// Don't call any of the IBrowserHost.DragSource*Ended* methods after returning false.
        /// Return true to handle the drag operation. Call <see cref="IBrowserHost.DragSourceEndedAt"/> and <see cref="IBrowserHost.DragSourceSystemDragEnded"/> either synchronously or asynchronously to inform
        /// the web view that the drag operation has ended. 
        /// </summary>
        /// <param name="dragData">drag data</param>
        /// <param name="mask">operation mask</param>
        /// <param name="x">combined x and y provide the drag start location in screen coordinates</param>
        /// <param name="y">combined x and y provide the drag start location in screen coordinates</param>
        /// <returns>Return false to abort the drag operation.</returns>
        public virtual bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
        {
            return false;
        }

        /// <summary>
        /// Called when the web view wants to update the mouse cursor during a drag &amp; drop operation.
        /// </summary>
        /// <param name="operation">describes the allowed operation (none, move, copy, link). </param>
        public virtual void UpdateDragCursor(DragOperationsMask operation)
        {

        }

        /// <summary>
        /// Called when the browser wants to show or hide the popup widget.  
        /// </summary>
        /// <param name="show">The popup should be shown if show is true and hidden if show is false.</param>
        public virtual void OnPopupShow(bool show)
        {
            PopupOpen = show;
        }

        /// <summary>
        /// Called when the browser wants to move or resize the popup widget. 
        /// </summary>
        /// <param name="rect">contains the new location and size in view coordinates. </param>
        public virtual void OnPopupSize(Rect rect)
        {
            popupPosition.X = rect.X;
            popupPosition.Y = rect.Y;
            popupSize.Width = rect.Width;
            popupSize.Height = rect.Height;
        }

        /// <summary>
        /// Called when the IME composition range has changed.
        /// </summary>
        /// <param name="selectedRange">is the range of characters that have been selected</param>
        /// <param name="characterBounds">is the bounds of each character in view coordinates.</param>
        public virtual void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
        {

        }

        /// <summary>
        /// Called when an on-screen keyboard should be shown or hidden for the specified browser. 
        /// </summary>
        /// <param name="browser">the browser</param>
        /// <param name="inputMode">specifies what kind of keyboard should be opened. If <see cref="TextInputMode.None"/>, any existing keyboard for this browser should be hidden.</param>
        public virtual void OnVirtualKeyboardRequested(IBrowser browser, TextInputMode inputMode)
        {
            keyboard?.Invoke(inputMode);
        }
    }


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
    public class WebBrowser : AssetProvider<RTexture2D>, IRenderObject, IKeyboardStealer, IAudioHandler,IAudioSource
    {
        private Size popupSize;
        private Point popupPosition;
        public bool Threaded => true;

        public RenderFrequency renderFrac => renderFrequency.value;

        public bool IsActive => loaded;

        public int ChannelCount => (int)audioType.value;

        RollBuffer frameInputBuffer;

        public byte[] FrameInputBuffer => frameInputBuffer.array;

        public Sync<RenderFrequency> renderFrequency;
        public Sync<Vector2u> scale;
        public SyncRef<IinputPlane> imputPlane;
        public Sync<string> path;
        public Driver<string> title;
        public Sync<bool> globalAudio;
        public Sync<bool> noKeyboard;
        public Sync<AudioType> audioType;

        private bool loaded;
        private bool updateUrl = false;

        ChromiumWebBrowser browser;
        public override void OnAttach()
        {
            base.OnAttach();
       
        }
        IAudioHandler audio;
        public override void onLoaded()
        {
            base.onLoaded();
            frameInputBuffer = new RollBuffer(engine.audioManager.AudioFrameSizeInBytes*ChannelCount);
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
            browser = new ChromiumWebBrowser(path.value, null, null, false,onAfterBrowserCreated);
            browser.MenuHandler = new CustomMenuHandler();
            audio = browser.AudioHandler;
            browser.AddressChanged += Browser_AddressChanged;
            browser.TitleChanged += Browser_TitleChanged;
            var hander = new RenderHandler(browser);
            hander.keyboard += Hander_keyboard;
            hander.cursorChange += Hander_cursorChange;
            browser.RenderHandler = hander;
            GlobalAudio_Changed(null);
            browser.CreateBrowser();
            browser.Size = new System.Drawing.Size { Width = (int)scale.value.x, Height = (int)scale.value.y };            
            loaded = true;
        }

        private void Hander_cursorChange(CursorType obj)
        {
            if (imputPlane.target == null) return;
            imputPlane.target.SetCursor(Input.CursorsEnumCaster.CursorType(obj));
        }

        private void Hander_keyboard(TextInputMode obj)
        {
            switch (obj)
            {
                case TextInputMode.None:
                    LoseKeyboard();
                    break;
                default:
                    TakeKeyboard();
                    break;
            }
        }

        private void TakeKeyboard()
        {
            if (noKeyboard.value) return;
            input.keyboard = this;
            if (imputPlane.target != null)
            {
                imputPlane.target.StopMouse = true;
            }
        }
        private void LoseKeyboard()
        {
            if (input.keyboard == this)
            {
                input.keyboard = null;
            }
            if (imputPlane.target != null)
            {
                imputPlane.target.StopMouse = false;
            }
        }
        private void Browser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            if (title.Linked)
            {
                title.Drivevalue = e.Title;
            }
        }

        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            path.value = e.Address;
        }

        private void onAfterBrowserCreated(IBrowser obj)
        {
            if (updateUrl)
            {
                browser.LoadUrlAsync(path.value);
            }
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
            scale = new Sync<Vector2u>(this, newRefIds);
            imputPlane = new SyncRef<IinputPlane>(this, newRefIds);
            scale.value = new Vector2u(600, 600);
            scale.Changed += onScaleChange;
            path = new Sync<string>(this, newRefIds);
            globalAudio = new Sync<bool>(this, newRefIds);
            audioType = new Sync<AudioType>(this, newRefIds);
            audioType.value = AudioType.LayoutMono;
            audioType.Changed += AudioType_Changed;
            globalAudio.Changed += GlobalAudio_Changed;
            path.value = "https://www.youtube.com/watch?v=Rp6ehxZvvM4";
            path.Changed += Path_Changed;
            //path.value = "https://google.com/";
            //path.value = "https://g.co/arts/xoCTBcR4S3MD8QPE6";
            //path.value = "https://www.vsynctester.com/testing/mouse.html";
            title = new Driver<string>(this, newRefIds);
            noKeyboard = new Sync<bool>(this, newRefIds);

        }

        private void AudioType_Changed(IChangeable obj)
        {
            loaded = false;
            browser.Dispose();
            onLoaded();
        }

        private void GlobalAudio_Changed(IChangeable obj)
        {
            if (browser == null) return;
            if (globalAudio.value) 
            {
                frameInputBuffer.Push(new byte[engine.audioManager.AudioFrameSizeInBytes * ChannelCount]);
                browser.AudioHandler = null;
            }
            else
            {
                browser.AudioHandler = this;
            }
        }

        private void onScaleChange(IChangeable obj)
        {
            if (!loaded) return;
            browser.Size = new Size { Width = (int)scale.value.x, Height = (int)scale.value.y };
        }

        private void Path_Changed(IChangeable obj)
        {
            if (!loaded) return;
            if (browser.IsBrowserInitialized)
            {
                if(path.value != browser.Address)
                      browser.LoadUrlAsync(path.value);
            }
            else
            {
                if (path.value != browser.Address)
                    updateUrl = true;
            }
        }

        TextureView view;
        Texture target;
        public void Render()
        {
            if (!loaded) return;
            startRenderTask();
            updateInpute();
        }
        public override void LoadListObject()
        {
            try
            {
               world.updateLists.trenderObject.SafeAdd(this);
            }
            catch { }
        }

        public override void RemoveListObject()
        {
            try
            {
                world.updateLists.trenderObject.Remove(this);
            }
            catch { }
        }

        private Task lastTask;

        public void startRenderTask()
        {
            if (lastTask != null) 
            {
                if ((!(lastTask.IsFaulted))&&!lastTask.IsCompleted)
                {
                    return;
                }
            }
            lastTask = Task.Run(RenderTask);
        }

        private void RenderTask()
        {
            try
            {
                if (view == null)
                {
                    var thing = ScreenshotOrNull(browser, PopupBlending.Main);
                    if (thing == null) return;
                    target = (new ImageSharpTexture(thing.ToImageSharpImage<Rgba32>(), false)).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                    view = engine.renderManager.gd.ResourceFactory.CreateTextureView(target);
                    var e = new RTexture2D(view);
                    e.addDisposable(target);
                    e.addDisposable(view);
                    load(e, true);
                }
                else
                {
                    //target.UpdateTexture((new ImageSharpTexture(ScreenshotOrNull(browser,PopupBlending.Main).ToImageSharpImage<Rgba32>(),false)), engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                    target.UpdateTextureBmp(ScreenshotOrNull(browser,PopupBlending.Main), engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("WebBrowser Render Error" + e.ToString());
            }
            lastTask = null;
        }

        public Bitmap ScreenshotOrNull(ChromiumWebBrowser browser,PopupBlending blend = PopupBlending.Main)
        {
            if (browser.RenderHandler == null)
            {
                throw new NullReferenceException("RenderHandler cannot be null. Use DefaultRenderHandler unless implementing your own");
            }

            var renderHandler = browser.RenderHandler as RenderHandler;

            if (renderHandler == null)
            {
                throw new Exception("ScreenshotOrNull and ScreenshotAsync can only be used in combination with the DefaultRenderHandler");
            }

            lock (renderHandler.BitmapLock)
            {
                if (blend == PopupBlending.Main)
                {
                    return renderHandler.BitmapBuffer.CreateBitmap();
                }

                if (blend == PopupBlending.Popup)
                {
                    return renderHandler.PopupOpen ? renderHandler.PopupBuffer.CreateBitmap() : null;
                }


                var bitmap = renderHandler.BitmapBuffer.CreateBitmap();

                if (renderHandler.PopupOpen && bitmap != null)
                {
                    var popup = renderHandler.PopupBuffer.CreateBitmap();
                    if (popup == null)
                    {
                        return bitmap;
                    }
                    return MergeBitmaps(bitmap, popup, renderHandler.PopupPosition);
                }

                return bitmap;
            }
        }
        private static Bitmap MergeBitmaps(Bitmap firstBitmap, Bitmap secondBitmap, Point secondBitmapPosition)
        {
            var mergedBitmap = new Bitmap(firstBitmap.Width, firstBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(mergedBitmap))
            {
                g.DrawImage(firstBitmap, new System.Drawing.Rectangle(0, 0, firstBitmap.Width, firstBitmap.Height));
                g.DrawImage(secondBitmap, new System.Drawing.Rectangle(secondBitmapPosition.X, secondBitmapPosition.Y, secondBitmap.Width, secondBitmap.Height));
            }
            return mergedBitmap;
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
                else if (lp >= 109 && lp <= 118)
                {
                    k.WindowsKeyCode = lp + (-109 + 48);
                }
                else if(lp >= 67 && lp <= 118) 
                {
                    k.WindowsKeyCode = lp + (-67 + 76);
                }
                else if(lp >= 10 && lp <= 33)
                {
                    k.WindowsKeyCode = lp + (-10 + 112);
                }
                else if(lp == 49)
                {
                    k.WindowsKeyCode = lp + (-49 + 13);
                }
                else if(lp == 53)
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
            switch (audioType.value)
            {
                case AudioType.LayoutUnsupported:
                    parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutUnsupported;
                    break;
                case AudioType.LayoutMono:
                    parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutMono;
                    break;
                case AudioType.LayoutStereo:
                    parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutStereo;
                    break;
                case AudioType.LayoutSurround:
                    parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutSurround;
                    break;
                default:
                    parameters.ChannelLayout = CefSharp.Enums.ChannelLayout.LayoutUnsupported;
                    break;
            }
            parameters.FramesPerBuffer = engine.audioManager.AudioFrameSize;
            parameters.SampleRate = engine.audioManager.SamplingRate;
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
                int chan = ChannelCount;
                int size = noOfFrames*sizeof(float) * chan;
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
                frameInputBuffer.Push(samples);
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
