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

namespace RhubarbEngine.Components.Rendering
{
	[Category(new string[] { "Rendering" })]
	public class Camera2D : AssetProvider<RTexture2D>, IRenderObject
	{
        public Sync<RenderFrequency> renderFrequency;

        public Sync<Vector2u> scale;

        public Sync<bool> renderShadowLayer;

        public Sync<Quaternionf> addedRotation;

        public Sync<bool> depthView;

        public Sync<float> fieldOfView;

        public Sync<float> nearPlaneDistance;

        public Sync<float> farPlaneDistance;

        public SyncRefList<Renderable> excludedsRenderObjects;

        private Framebuffer _framebuffer;

        private bool _renderLoaded = false;

        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);
            renderFrequency = new Sync<RenderFrequency>(this, newRefIds);
            scale = new Sync<Vector2u>(this, newRefIds)
            {
                Value = new Vector2u(600, 600)
            };
            scale.Changed += OnScaleChange;
            renderShadowLayer = new Sync<bool>(this, newRefIds);
            addedRotation = new Sync<Quaternionf>(this, newRefIds)
            {
                Value = Quaternionf.Identity
            };
            depthView = new Sync<bool>(this, newRefIds);
            depthView.Changed += DepthView_Changed;
            fieldOfView = new Sync<float>(this, newRefIds)
            {
                Value = 60f
            };
            fieldOfView.Changed += Proj_Changed;
            nearPlaneDistance = new Sync<float>(this, newRefIds)
            {
                Value = 0.01f
            };
            nearPlaneDistance.Changed += Proj_Changed;
            farPlaneDistance = new Sync<float>(this, newRefIds)
            {
                Value = 1000f
            };
            farPlaneDistance.Changed += Proj_Changed;
            excludedsRenderObjects = new SyncRefList<Renderable>(this, newRefIds);
        }

        public RenderFrequency RenderFrac
        {
            get
            {
                return renderFrequency.Value;
            }
        }

        public bool Threaded
        {
            get
            {
               return true;
            }
        }

        private RenderQueue _renderQueue;

        public override void OnLoaded()
        {
            base.OnLoaded();
            if (!Engine.Rendering)
            {
                return;
            }
            _renderQueue = new RenderQueue();
            _renderCL = Engine.RenderManager.Gd.ResourceFactory.CreateCommandList();
            LoadData();
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

        private RTexture2D _depth;

        private RTexture2D _color;

        private void LoadRTexture2D()
        {
            if (depthView.Value)
            {
                Load(_depth);
            }
            else
            {
                Load(_color);
            }
        }

        private Matrix4x4 _proj;

        public override void LoadListObject()
        {
            try
            {
                World.updateLists.renderObject.SafeAdd(this);
            }
            catch { }
        }

        public override void RemoveListObject()
        {
            try
            {
                World.updateLists.renderObject.Remove(this);
            }
            catch { }
        }


        private void BuildMainRenderQueue(Matrix4x4 view)
        {
            _renderQueue.Clear();
            var frustum = new Utilities.BoundingFrustum(_proj);
            Engine.WorldManager.AddToRenderQueue(_renderQueue, Entity.remderlayer.Value, frustum, view);
            _renderQueue.OrderExsclude((obj) => excludedsRenderObjects.Contains(obj.ReferenceID));
        }


        private void LoadData()
        {
            if (!Engine.Rendering)
            {
                return;
            }
            try
            {
                Logger.Log("Loading Camera2d");
                if (scale.Value.x < 2 || scale.Value.y < 2)
                {
                    throw new Exception("Camera2D FrameBuffer too Small");
                }
                Proj_Changed(null);
                _framebuffer = CreateFramebuffer(scale.Value.x, scale.Value.y);
                var target = _framebuffer.ColorTargets[0].Target;
                var depthtarget = _framebuffer.DepthTarget.Value.Target;
                var view = Engine.RenderManager.Gd.ResourceFactory.CreateTextureView(target);
                var depthView = Engine.RenderManager.Gd.ResourceFactory.CreateTextureView(depthtarget);
                _color = new RTexture2D(view);
                _depth = new RTexture2D(depthView);
                LoadRTexture2D();
                _renderLoaded = true;
            }
            catch (Exception e)
            {
                Logger.Log("Camra2D Error When Loading Error" + e.ToString(), true);
            }
        }
        private void OnScaleChange(IChangeable val)
        {
            if ((_framebuffer != null) && _renderLoaded)
            {
                _renderLoaded = false;
                Load(null);
                _framebuffer?.Dispose();
                _depth?.Dispose();
                _color?.Dispose();
                _depth = null;
                _color = null;
                LoadData();
            }
        }



        private void Proj_Changed(IChangeable obj)
        {
            _proj = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 180) * fieldOfView.Value, (float)scale.Value.x/(float)scale.Value.y, nearPlaneDistance.Value, farPlaneDistance.Value);
        }

        private void DepthView_Changed(IChangeable obj)
        {
            if (!Engine.Rendering)
            {
                return;
            }
            LoadRTexture2D();
        }

        public Camera2D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public Camera2D()
		{
		}

        private CommandList _renderCL;

        public void Render()
		{
            if (_renderLoaded && _renderCL is not null)
            {
                try
                {
                    var trans = Entity.GlobalTrans() * Matrix4x4.CreateFromQuaternion((Quaternion)addedRotation.Value);
                    Matrix4x4.Invert(trans, out var invert);
                    var view = Matrix4x4.CreateScale(1f) * invert;
                    BuildMainRenderQueue(view);
                    _renderCL.Begin();
                    _renderCL.SetFramebuffer(_framebuffer);
                    _renderCL.ClearDepthStencil(1f);
                    _renderCL.ClearColorTarget(0, RgbaFloat.White);
                    if (renderShadowLayer.Value)
                    {
                        foreach (var renderObj in _renderQueue.Renderables)
                        {
                            renderObj.RenderShadow(Engine.RenderManager.Gd, _renderCL, new UBO(
                            _proj,
                            view,
                            renderObj.Entity.GlobalTrans()));
                        }
                    }
                    else
                    {
                        foreach (var renderObj in _renderQueue.Renderables)
                        {
                            renderObj.Render(Engine.RenderManager.Gd, _renderCL, new UBO(
                            _proj,
                            view,
                            renderObj.Entity.GlobalTrans()));
                        }
                    }

                    _renderCL.End();
                    Engine.RenderManager.Gd.SubmitCommands(_renderCL);

                }
                catch (Exception e)
                {
                    Logger.Log($"Error with render with camera {e}", true);
                }
            }
		}

        public override void Dispose()
        {

            if ((_framebuffer != null) && _renderLoaded)
            {
                _renderLoaded = false;
                Load(null);
                _framebuffer?.Dispose();
                _depth?.Dispose();
                _color?.Dispose();
                _depth = null;
                _color = null;
            }
            _renderCL?.Dispose();
            base.Dispose();
        }
    }
}
