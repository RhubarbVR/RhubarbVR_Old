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
    public class TextRender : Renderable
    {
        public Sync<uint> RenderOrderOffset;

        public Sync<float> LetterSpacing;

        public Sync<string> Text;

        public Sync<Vector2f> Pos;

        public Sync<Colorf> Color;

        public AssetRef<RFont> Font;

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
            RenderOrderOffset = new Sync<uint>(this, newRefIds);
            Font = new AssetRef<RFont>(this, newRefIds);
            Font.LoadChange += Font_LoadChange;
            Text = new Sync<string>(this, newRefIds) 
            {
                Value = "Hello World"
            };
            Pos = new Sync<Vector2f>(this, newRefIds);
            LetterSpacing = new Sync<float>(this, newRefIds)
            {
                Value = 0.01f
            };
            Color = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.White
            };
        }

        private void Font_LoadChange(RFont obj)
        {
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
                }
                else if (Engine.Rendering)
                {
                    _textRenderer = new VeldridTextRenderer(Engine.RenderManager.Gd, obj.font, Engine.RenderManager.VrContext.LeftEyeFramebuffer.OutputDescription);
                }
            }catch(Exception e)
            {
                Logger.Log("Failed to load TextRender " + e.ToString());
            }
        }

        public override void OnLoaded()
        {
            base.OnLoaded();
            if (Engine.Rendering && Font.Asset is not null)
            {
                _textRenderer = new VeldridTextRenderer(Engine.RenderManager.Gd, Font.Asset.font, Engine.RenderManager.VrContext.LeftEyeFramebuffer.OutputDescription);
            }
        }

        private BoundingBox _changingBoundingBox = new()
        {
            Max = new Vector3(100)
        };

        public override BoundingBox BoundingBox
        {
            get
            {
                return _changingBoundingBox;
            }
        }

        public TextRender(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public TextRender()
        {
        }

        public override void Render(GraphicsDevice gd, CommandList cl, UBO ubo, Framebuffer framebuffer)
        {
            if(_textRenderer is null)
            {
                return;
            }
            _textRenderer.UpdateVeldridStuff(ubo.Projection * ubo.View * ubo.World, cl, framebuffer);
            _textRenderer.Update();
            _textRenderer.DrawText(Text.Value, (Vector2)Pos.Value, new SharpText.Core.Color(Color.Value.r, Color.Value.g, Color.Value.b, Color.Value.a), LetterSpacing.Value);
            _textRenderer.Draw();
            cl.SetFramebuffer(framebuffer);
        }

        public override void RenderShadow(GraphicsDevice gd, CommandList cl, UBO ubo, Framebuffer framebuffer)
        {
        }
        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return RenderOrderKey.Create(RenderOrderOffset.Value, BoundingBox.DistanceFromPoint((Vector3)Entity.GlobalPointToLocal((Vector3f)cameraPosition, false)));

        }
    }
}
