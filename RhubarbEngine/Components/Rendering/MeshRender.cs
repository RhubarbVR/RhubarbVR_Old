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

        public override BoundingBox BoundingBox
        {
            get
            {
                return Mesh.Asset.boundingBox;
            }
        }

        public override void buildSyncObjs(bool newRefIds)
		{
			Mesh = new AssetRef<RMesh>(this, newRefIds);
			Materials = new SyncAssetRefList<RMaterial>(this, newRefIds);
			Mesh.loadChange += LoadMesh;
			Materials.loadChange += LoadMaterial;
            RenderOrderOffset = new Sync<uint>(this, newRefIds)
            {
                Value = int.MaxValue
            };
        }

		private void LoadMesh(RMesh mesh)
		{
			if (_wvpBuffer == null)
            {
                return;
            }

            if (Mesh.Target == null)
			{
				logger.Log("no mesh provider");
				return;
			}
			if (Mesh.Target.value == null)
			{
				logger.Log("no mesh to load");
				logger.Log($"{Mesh.Value.getID()}");
			}
			else
			{
				_meshPieces = Mesh.Asset.meshPieces.ToArray();
			}
			CheckIsLoaded();
		}

		private void LoadMaterial(RMaterial mit)
		{
			if (_wvpBuffer == null)
            {
                return;
            }

            LoadAllMaterials();
			CheckIsLoaded();
		}

		private void LoadAllMaterials()
		{
			if (_wvpBuffer == null)
            {
                return;
            }

            logger.Log("load Materials");
			var factory = Gd.ResourceFactory;

            var positionLayoutDesc = new VertexLayoutDescription(
				new VertexElementDescription[]
				  {
					new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				   });

			var texCoordLayoutDesc = new VertexLayoutDescription(
				new VertexElementDescription[]
				{
					new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
				});

			foreach (var mit in Materials)
			{
				if (mit != null)
				{
					if (mit.Shader != null)
					{
						if (mit.Shader.Asset != null)
						{
							if (mit.Shader.Asset.shaderLoaded)
							{

								var mainPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
				BlendStateDescription.SingleAlphaBlend,
				DepthStencilStateDescription.DepthOnlyLessEqual,
				RasterizerStateDescription.CullNone,
				PrimitiveTopology.TriangleList,
				new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.mainVertShader, mit.Shader.Asset.mainFragShader }),
				mit.Shader.Asset.mainresourceLayout,
				engine.renderManager.vrContext.LeftEyeFramebuffer.OutputDescription));
								AddDisposable(mainPipeline);
								_mainPipeline.Add(mainPipeline);


								var shadowPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
				BlendStateDescription.SingleAlphaBlend,
				DepthStencilStateDescription.DepthOnlyLessEqual,
				RasterizerStateDescription.CullNone,
				PrimitiveTopology.TriangleList,
				new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.shadowVertShader, mit.Shader.Asset.shadowFragShader }),
				mit.Shader.Asset.shadowresourceLayout,
				engine.renderManager.vrContext.LeftEyeFramebuffer.OutputDescription));
								AddDisposable(shadowPipeline);
								_shadowpipeline.Add(mainPipeline);


                                var mainResourceSetDescription = new ResourceSetDescription
                                {
                                    Layout = mit.Shader.Asset.mainresourceLayout
                                };
                                var mainBoundResources = new List<BindableResource>
                                {
                                    _wvpBuffer,
                                    Gd.Aniso4xSampler
                                };
                                mit.GetBindableResources(mainBoundResources, false);

								mainResourceSetDescription.BoundResources = mainBoundResources.ToArray();

								var mainRS = factory.CreateResourceSet(mainResourceSetDescription);
								AddDisposable(mainRS);

								_mainRS.Add(mainRS);


                                var shadowResourceSetDescription = new ResourceSetDescription
                                {
                                    Layout = mit.Shader.Asset.shadowresourceLayout
                                };
                                var shadowBoundResources = new List<BindableResource>
                                {
                                    _wvpBuffer,
                                    Gd.Aniso4xSampler
                                };
                                mit.GetBindableResources(shadowBoundResources, true);

								shadowResourceSetDescription.BoundResources = shadowBoundResources.ToArray();
								var shadowRS = factory.CreateResourceSet(shadowResourceSetDescription);
								AddDisposable(shadowRS);
								_shadowRS.Add(shadowRS);

								mit.BindableResourcesReload += ReloadResorseSet;
							}
						}
					}
				}
			}
			CheckIsLoaded();
		}

		public void ReloadResorseSet(RMaterial mit)
		{
			if (_wvpBuffer == null)
            {
                return;
            }

            var mitindex = Materials.indexOf(mit);
			if (mitindex == -1)
			{
				mit.BindableResourcesReload -= ReloadResorseSet;
				return;
			}
			var factory = Gd.ResourceFactory;
			if (mit.Shader.Asset != null)
			{
				if (mit.Shader.Asset.shaderLoaded)
				{
                    var mainResourceSetDescription = new ResourceSetDescription
                    {
                        Layout = mit.Shader.Asset.mainresourceLayout
                    };
                    var mainBoundResources = new List<BindableResource>
                    {
                        _wvpBuffer,
                        Gd.Aniso4xSampler
                    };
                    mit.GetBindableResources(mainBoundResources, false);

					mainResourceSetDescription.BoundResources = mainBoundResources.ToArray();
					var mainRS = factory.CreateResourceSet(mainResourceSetDescription);
					AddDisposable(mainRS);
					_mainRS[mitindex].Dispose();
					_mainRS[mitindex] = mainRS;

                    var shadowResourceSetDescription = new ResourceSetDescription
                    {
                        Layout = mit.Shader.Asset.shadowresourceLayout
                    };
                    var shadowBoundResources = new List<BindableResource>
                    {
                        _wvpBuffer,
                        Gd.Aniso4xSampler
                    };
                    mit.GetBindableResources(shadowBoundResources, true);

					shadowResourceSetDescription.BoundResources = shadowBoundResources.ToArray();
					var shadowRS = factory.CreateResourceSet(shadowResourceSetDescription);
					AddDisposable(shadowRS);
					_shadowRS[mitindex].Dispose();
					_shadowRS[mitindex] = shadowRS;
				}
			}
		}

		private void CheckIsLoaded()
		{
			_loaded = _meshPieces.Length > 0 && _mainRS.Count > 0 && _shadowRS.Count > 0;
		}

		private MeshPiece[] _meshPieces = new MeshPiece[0];
        private GraphicsDevice Gd
        {
            get
            {
                return engine.renderManager.gd;
            }
        }

        private readonly List<Pipeline> _mainPipeline = new List<Pipeline>();
		private readonly List<Pipeline> _shadowpipeline = new List<Pipeline>();
		private DeviceBuffer _wvpBuffer;
		private readonly List<ResourceSet> _mainRS = new List<ResourceSet>();
		private readonly List<ResourceSet> _shadowRS = new List<ResourceSet>();
		private bool _loaded;

		public override void Render(GraphicsDevice gd, CommandList cl, UBO ubo)
		{
			try
			{
				InternalRender(gd, cl, ubo, false);
			}
			catch { }
		}

		public override void RenderShadow(GraphicsDevice gd, CommandList cl, UBO ubo)
		{
			try
			{
				InternalRender(gd, cl, ubo, true);
			}
			catch { }
		}

#pragma warning disable IDE0060 // Remove unused parameter
        public void InternalRender(GraphicsDevice gd, CommandList cl, UBO ubo, bool shadow = false)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			if (!_loaded)
			{
				return;
			}
			cl.UpdateBuffer(_wvpBuffer, 0, ubo);
			var Length = Math.Max(_meshPieces.Length, Materials.Length);
			for (var i = 0; i < Length; i++)
			{
				var a = i % _meshPieces.Length;
				var b = i % Materials.Length;
				var piece = _meshPieces[a];
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
			return RenderOrderKey.Create(RenderOrderOffset.Value, BoundingBox.DistanceFromPoint((Vector3)entity.GlobalPointToLocal((Vector3f)cameraPosition, false)));
		}


		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{

		}
		public override void onLoaded()
		{
			_wvpBuffer = engine.renderManager.gd.ResourceFactory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
			AddDisposable(_wvpBuffer);
			Logger.Log("Loading Mesh Render");
			LoadMesh(null);
			LoadMaterial(null);
		}
		public override void onChanged()
		{
		}
		public MeshRender(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MeshRender()
		{
		}
	}
}
