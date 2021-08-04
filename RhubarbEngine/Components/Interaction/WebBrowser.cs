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
using CefSharp;
using CefSharp.OffScreen;
using Veldrid.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RhubarbEngine.Components.Interaction
{

    [Category(new string[] { "Interaction" })]
    public class WebBrowser : AssetProvider<RTexture2D>, IRenderObject, KeyboardStealer
    {
        public RenderFrequency renderFrac => renderFrequency.value;

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
            browser = new ChromiumWebBrowser("https://google.com");
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
            path.value = "https://google.com/";
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

        public void Render()
        {
            if (!loaded) return;
            Texture target = (new ImageSharpTexture(browser.ScreenshotOrNull(PopupBlending.Main).ToImageSharpImage<Rgba32>())).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
            TextureView view = engine.renderManager.gd.ResourceFactory.CreateTextureView(target);
            var e = new RTexture2D(view);
            e.addDisposable(target);
            e.addDisposable(view);
            load(e, true);
            updateInpute();
        }

        public void updateInpute()
        {
            if (imputPlane.target == null) return;
            var imp = imputPlane.target;
            foreach (var item in imp.KeyEvents)
            {
                CefSharp.KeyEvent k = new CefSharp.KeyEvent();
                var lp = (int)item.Key;
                if (lp >= 83 && lp <= 108)
                {
                    k.WindowsKeyCode = lp + (-83 + 65);
                }
                k.Modifiers = CefEventFlags.None;
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Alt)) >= 0f)
                {
                    k.Modifiers |= CefEventFlags.AltDown;
                }
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Control)) >= 0f)
                {
                    k.Modifiers |= CefEventFlags.ControlDown;
                }
                if ((((int)item.Modifiers) & ((int)ModifierKeys.Shift)) >= 0f)
                {
                    k.Modifiers |= CefEventFlags.ShiftDown;
                }
                k.Type = (item.Down) ? KeyEventType.KeyDown : KeyEventType.KeyUp;
                k.IsSystemKey = false;
                browser.GetBrowser().GetHost().SendKeyEvent(k);
            }

        }

        public WebBrowser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public WebBrowser()
        {
        }
    }
}
