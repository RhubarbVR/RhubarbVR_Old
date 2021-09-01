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
using g3;
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
    public class MeshRender : Renderable
    {

        public AssetRef<RMesh> Mesh;
        public SyncAssetRefList<RMaterial> Materials;

        public Sync<uint> RenderOrderOffset;

        public override BoundingBox BoundingBox => Mesh.Asset.boundingBox;


        public override void buildSyncObjs(bool newRefIds)
        {
            Mesh = new AssetRef<RMesh>(this, newRefIds);
            Materials = new SyncAssetRefList<RMaterial>(this, newRefIds);
            Mesh.loadChange += loadMesh;
            Materials.loadChange += loadMaterial;
            RenderOrderOffset = new Sync<uint>(this, newRefIds);
            RenderOrderOffset.value = int.MaxValue;
        }

        private void loadMesh(RMesh mesh)
        {
            if (_wvpBuffer == null) return;
            if(Mesh.target == null)
            {
                logger.Log("no mesh provider");
                return;
            }
            if (Mesh.target.value == null)
            {
                logger.Log("no mesh to load");
                logger.Log($"{Mesh.value.getID()}");
            }
            else
            {
                _meshPieces = Mesh.Asset.meshPieces.ToArray();
            }
            checkIsLoaded();
        }

        private void loadMaterial(RMaterial mit)
        {
            if (_wvpBuffer == null) return;
            loadAllMaterials();
            checkIsLoaded();
        }

        private void loadAllMaterials()
        {
            if (_wvpBuffer == null) return;
            logger.Log("load Materials");
            ResourceFactory factory = _gd.ResourceFactory;

            VertexLayoutDescription positionLayoutDesc = new VertexLayoutDescription(
                new VertexElementDescription[]
                  {
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                   });

            VertexLayoutDescription texCoordLayoutDesc = new VertexLayoutDescription(
                new VertexElementDescription[]
                {
                    new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                });

            foreach (RMaterial mit in Materials)
            {
                if (mit != null)
                {
                    if (mit.Shader != null)
                    {
                        if (mit.Shader.Asset != null)
                        {
                            if (mit.Shader.Asset.shaderLoaded)
                            {

                                Pipeline mainPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.mainVertShader, mit.Shader.Asset.mainFragShader }),
                mit.Shader.Asset.mainresourceLayout,
                engine.renderManager.vrContext.LeftEyeFramebuffer.OutputDescription));
                                addDisposable(mainPipeline);
                                _mainPipeline.Add(mainPipeline);


                                Pipeline shadowPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.shadowVertShader, mit.Shader.Asset.shadowFragShader }),
                mit.Shader.Asset.shadowresourceLayout,
                engine.renderManager.vrContext.LeftEyeFramebuffer.OutputDescription));
                                addDisposable(shadowPipeline);
                                _shadowpipeline.Add(mainPipeline);


                                ResourceSetDescription mainResourceSetDescription = new ResourceSetDescription();
                                mainResourceSetDescription.Layout = mit.Shader.Asset.mainresourceLayout;
                                List<BindableResource> mainBoundResources = new List<BindableResource>();
                                mainBoundResources.Add(_wvpBuffer);
                                mainBoundResources.Add(_gd.Aniso4xSampler);
                                mit.getBindableResources(mainBoundResources, false);

                                mainResourceSetDescription.BoundResources = mainBoundResources.ToArray();

                                ResourceSet mainRS = factory.CreateResourceSet(mainResourceSetDescription);
                                addDisposable(mainRS);

                                _mainRS.Add(mainRS);


                                ResourceSetDescription shadowResourceSetDescription = new ResourceSetDescription();
                                shadowResourceSetDescription.Layout = mit.Shader.Asset.shadowresourceLayout;
                                List<BindableResource> shadowBoundResources = new List<BindableResource>();
                                shadowBoundResources.Add(_wvpBuffer);
                                shadowBoundResources.Add(_gd.Aniso4xSampler);
                                mit.getBindableResources(shadowBoundResources, true);

                                shadowResourceSetDescription.BoundResources = shadowBoundResources.ToArray();
                                ResourceSet shadowRS = factory.CreateResourceSet(shadowResourceSetDescription);
                                addDisposable(shadowRS);
                                _shadowRS.Add(shadowRS);

                                mit.BindableResourcesReload += ReloadResorseSet;
                            }
                        }
                    }
                }
            }
            checkIsLoaded();
        }

        public void ReloadResorseSet(RMaterial mit)
        {
            if (_wvpBuffer == null) return;
            int mitindex = Materials.indexOf(mit);
            if (mitindex == -1)
            {
                mit.BindableResourcesReload -= ReloadResorseSet;
                return;
            }
            ResourceFactory factory = _gd.ResourceFactory;
                if (mit.Shader.Asset != null)
                {
                    if (mit.Shader.Asset.shaderLoaded)
                    {
                        ResourceSetDescription mainResourceSetDescription = new ResourceSetDescription();
                        mainResourceSetDescription.Layout = mit.Shader.Asset.mainresourceLayout;
                        List<BindableResource> mainBoundResources = new List<BindableResource>();
                        mainBoundResources.Add(_wvpBuffer);
                        mainBoundResources.Add(_gd.Aniso4xSampler);
                        mit.getBindableResources(mainBoundResources, false);

                        mainResourceSetDescription.BoundResources = mainBoundResources.ToArray();
                        ResourceSet mainRS = factory.CreateResourceSet(mainResourceSetDescription);
                        addDisposable(mainRS);
                         _mainRS[mitindex].Dispose();
                       _mainRS[mitindex] = mainRS;

                       ResourceSetDescription shadowResourceSetDescription = new ResourceSetDescription();
                        shadowResourceSetDescription.Layout = mit.Shader.Asset.shadowresourceLayout;
                        List<BindableResource> shadowBoundResources = new List<BindableResource>();
                        shadowBoundResources.Add(_wvpBuffer);
                        shadowBoundResources.Add(_gd.Aniso4xSampler);
                        mit.getBindableResources(shadowBoundResources, true);

                        shadowResourceSetDescription.BoundResources = shadowBoundResources.ToArray();
                        ResourceSet shadowRS = factory.CreateResourceSet(shadowResourceSetDescription);
                       addDisposable(shadowRS);
                    _shadowRS[mitindex].Dispose();
                    _shadowRS[mitindex] = shadowRS;
                    }
                }
        }

        private void checkIsLoaded()
        {
            loaded = _meshPieces.Length > 0 && _mainRS.Count > 0 && _shadowRS.Count > 0;
        }

        private MeshPiece[] _meshPieces = new MeshPiece[0];
        private GraphicsDevice _gd => engine.renderManager.gd;
        private List<Pipeline> _mainPipeline = new List<Pipeline>();
        private List<Pipeline> _shadowpipeline = new List<Pipeline>();
        private DeviceBuffer _wvpBuffer;
        private List<ResourceSet> _mainRS = new List<ResourceSet>();
        private List<ResourceSet> _shadowRS = new List<ResourceSet>();
        private bool loaded;

        public override void Render(GraphicsDevice gd, CommandList cl, UBO ubo)
        {
            try
            {
                internalRender(gd, cl, ubo, false);
            }
            catch { }
        }

        public override void RenderShadow(GraphicsDevice gd, CommandList cl, UBO ubo)
        {
            try 
            { 
            internalRender(gd, cl, ubo, true);
            }
            catch { }
        }

        public void internalRender(GraphicsDevice gd, CommandList cl, UBO ubo,bool shadow = false)
        {
            if (!loaded)
            {
                return;
            }
            cl.UpdateBuffer(_wvpBuffer, 0, ubo);
            int Length = Math.Max(_meshPieces.Length, Materials.Length);
            for (int i = 0; i < Length; i++)
            {
                int a = i %_meshPieces.Length;
                int b = i % Materials.Length;
                MeshPiece piece = _meshPieces[a];
                cl.SetPipeline((shadow) ? _shadowpipeline[b] : _mainPipeline[b]);
                cl.SetVertexBuffer(0, piece.Positions);
                cl.SetVertexBuffer(1, piece.TexCoords);
                cl.SetIndexBuffer(piece.Indices, IndexFormat.UInt32);
                cl.SetGraphicsResourceSet(0, (shadow) ? _shadowRS[b] : _mainRS[b]);
                cl.DrawIndexed(piece.IndexCount);
            }
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
           return RenderOrderKey.Create(RenderOrderOffset.value, BoundingBox.DistanceFromPoint((Vector3)entity.GlobalPointToLocal((Vector3f)cameraPosition, false)));
        }


        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
           
        }
        public override void onLoaded()
        {
            _wvpBuffer = engine.renderManager.gd.ResourceFactory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            addDisposable(_wvpBuffer);
            Logger.Log("Loading Mesh Render");
            loadMesh(null);
            loadMaterial(null);
        }
        public override void onChanged()
        {
        }
        public MeshRender(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public MeshRender()
        {
        }
    }
}
