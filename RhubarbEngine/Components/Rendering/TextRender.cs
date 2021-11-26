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
using RhubarbEngine.Render;
using RhubarbEngine.World.Asset;
using RNumerics;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;
using SharpText.Veldrid;

namespace RhubarbEngine.Components.Rendering
{
    [Category(new string[] { "Rendering" })]
    public class TextRender : AssetProvider<RTexture2D>
    {
        public Sync<float> LetterSpacing;

        public Sync<string> Text;

        public Sync<Vector2f> Pos;

        public Sync<Vector2u> Scale;

        public Sync<Colorf> Color;

        public AssetRef<RFont> Font;

        private Framebuffer _framebuffer;

        private CommandList _commandList;

        private VeldridTextRenderer _textRenderer;

        public override void Dispose()
        {
            _textRenderer?.Dispose();
            _textRenderer = null;
            base.Dispose();
        }

        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);
            Font = new AssetRef<RFont>(this, newRefIds);
            Font.LoadChange += Font_LoadChange;
            Text = new Sync<string>(this, newRefIds) 
            {
                Value = "Hello World"
            };
            Text.Changed += Val_Changed;
            Pos = new Sync<Vector2f>(this, newRefIds);
            Pos.Changed += Val_Changed;
            Scale = new Sync<Vector2u>(this, newRefIds)
            {
                Value = new Vector2u(256, 64)
            };
            Scale.Changed += Scale_Changed;
            LetterSpacing = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
            LetterSpacing.Changed += Val_Changed;
            Color = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.White
            };
            Color.Changed += Val_Changed;
        }

        private void Val_Changed(IChangeable obj)
        {
            if(_commandList is not null && _textRenderer is not null)
            {
                Render();
            }
        }

        private void Scale_Changed(IChangeable obj)
        {
            ReloadFrameBuffer();
        }

        private Framebuffer CreateFramebuffer(uint width, uint height)
        {
            var factory = Engine.RenderManager.Gd.ResourceFactory;
            var colorTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
                TextureUsage.RenderTarget | TextureUsage.Sampled
                ));
            var depthTarget = factory.CreateTexture(TextureDescription.Texture2D(
                width, height,
                1, 1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil | TextureUsage.Sampled));
            return factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
        }

        private void Font_LoadChange(RFont obj)
        {
            if(_commandList is null)
            {
                return;
            }
            try
            {
                if (obj is null)
                {
                    _textRenderer = null;
                    return;
                }
                if (_textRenderer is not null)
                {
                    _textRenderer.UpdateFont(obj.font);
                    Render();
                }
                else if (Engine.Rendering)
                {
                    if (obj.font is not null)
                    {
                        _textRenderer = new VeldridTextRenderer(Engine.RenderManager.Gd, obj.font, Engine.RenderManager.VrContext.LeftEyeFramebuffer.OutputDescription);
                        _textRenderer.UpdateVeldridStuff(_commandList, _framebuffer, _framebuffer.Height, _framebuffer.Width);
                        Render();
                    }
                }
            }catch(Exception e)
            {
                Logger.Log("Failed to load TextRender " + e.ToString());
            }
        }

        private void ReloadFrameBuffer()
        {
            if (!Engine.Rendering)
            {
                return;
            }
            Load(null);
            _commandList?.Dispose();
            _framebuffer?.Dispose();
            _commandList = Engine.RenderManager.Gd.ResourceFactory.CreateCommandList();
            _framebuffer = CreateFramebuffer(Scale.Value.x, Scale.Value.y);
            if(_textRenderer is null)
            {
                Font_LoadChange(null);
            }
            else
            {
                _textRenderer.UpdateVeldridStuff(_commandList, _framebuffer, _framebuffer.Height, _framebuffer.Width);
            }
            var view = Engine.RenderManager.Gd.ResourceFactory.CreateTextureView(_framebuffer.ColorTargets[0].Target);
            Load(new RTexture2D(view));
            Render();
        }

        public override void OnLoaded()
        {
            base.OnLoaded();
            ReloadFrameBuffer();
        }

        public void Render()
        {
            if(_textRenderer is null)
            {
                return;
            }
            if (_commandList is null)
            {
                return; 
            }
            _commandList.Begin();
            _commandList.SetFramebuffer(_framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Clear);
            _commandList.ClearDepthStencil(1f);
            _textRenderer.Update();
            _textRenderer.DrawText(Text.Value, (Vector2)Pos.Value, new SharpText.Core.Color(Color.Value.r, Color.Value.g, Color.Value.b, Color.Value.a), LetterSpacing.Value);
            _textRenderer.Draw();
            _commandList.End();
            Engine.RenderManager.Gd.SubmitCommands(_commandList);
        }

        public TextRender(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public TextRender()
        {
        }
    }
}
