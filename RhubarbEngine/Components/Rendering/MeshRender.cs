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

        public override void BuildSyncObjs(bool newRefIds)
		{
			Mesh = new AssetRef<RMesh>(this, newRefIds);
			Materials = new SyncAssetRefList<RMaterial>(this, newRefIds);
			Mesh.LoadChange += LoadMesh;
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
				return;
			}
			if (Mesh.Target.Value != null)
			{
				_meshPieces = Mesh.Asset.MeshPieces.ToArray();
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

        private void ClearPipeLine()
        {
            for (var i = (_mainPipeline.Count - 1); i >= 0; i--)
            {
                var e = _mainPipeline[i];
                _mainPipeline.RemoveAt(i);
                e.Dispose();
                var s = _shadowpipeline[i];
                _shadowpipeline.RemoveAt(i);
                s.Dispose();

                _shadowPollyType.RemoveAt(i);
                _mainPollyType.RemoveAt(i);

                var ms = _mainRS[i];
                _mainRS.RemoveAt(i);
                ms.Dispose();

                var m = _shadowRS[i];
                _shadowRS.RemoveAt(i);
                m.Dispose();
            }
        }

		private void LoadAllMaterials()
		{
            if (Gd is null)
            {
                return;
            }
            if (_wvpBuffer == null)
            {
                return;
            }

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

            ClearPipeLine();

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
				mit.Shader.Asset.mainShader._blendStateDescription,
                mit.Shader.Asset.mainShader._depthStencilStateDescription,
                mit.Shader.Asset.mainShader._rasterizerStateDescription,
                mit.Shader.Asset.mainShader.primitiveTopology,
				new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.mainVertShader, mit.Shader.Asset.mainFragShader }),
				mit.Shader.Asset.mainresourceLayout,
				Engine.RenderManager.VrContext.LeftEyeFramebuffer.OutputDescription));
								AddDisposable(mainPipeline);
								_mainPipeline.Add(mainPipeline);
                                _mainPollyType.Add(mit.Shader.Asset.mainShader.primitiveTopology);

								var shadowPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                mit.Shader.Asset.shadowShader._blendStateDescription,
                mit.Shader.Asset.shadowShader._depthStencilStateDescription,
                mit.Shader.Asset.shadowShader._rasterizerStateDescription,
                mit.Shader.Asset.shadowShader.primitiveTopology,
                new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { mit.Shader.Asset.shadowVertShader, mit.Shader.Asset.shadowFragShader }),
				mit.Shader.Asset.shadowresourceLayout,
				Engine.RenderManager.VrContext.LeftEyeFramebuffer.OutputDescription));
								AddDisposable(shadowPipeline);
								_shadowpipeline.Add(mainPipeline);
                                _shadowPollyType.Add(mit.Shader.Asset.mainShader.primitiveTopology);

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

            var mitindex = Materials.IndexOf(mit);
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

		private MeshPiece[] _meshPieces = Array.Empty<MeshPiece>();
        private GraphicsDevice Gd
        {
            get
            {
                return Engine.RenderManager.Gd;
            }
        }

        private readonly List<Pipeline> _mainPipeline = new();
        private readonly List<PrimitiveTopology> _mainPollyType = new();
        private readonly List<Pipeline> _shadowpipeline = new();
        private readonly List<PrimitiveTopology> _shadowPollyType = new();
        private DeviceBuffer _wvpBuffer;
		private readonly List<ResourceSet> _mainRS = new();
		private readonly List<ResourceSet> _shadowRS = new();
		private bool _loaded;

		public override void Render(GraphicsDevice gd, CommandList cl, UBO ubo, Framebuffer framebuffer)
		{
			try
			{
				InternalRender(gd, cl, ubo, false);
			}
			catch { }
		}

		public override void RenderShadow(GraphicsDevice gd, CommandList cl, UBO ubo, Framebuffer framebuffer)
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
				cl.SetPipeline(shadow ? _shadowpipeline[b] : _mainPipeline[b]);
				cl.SetVertexBuffer(0, piece.Positions);
				cl.SetVertexBuffer(1, piece.TexCoords);
				cl.SetIndexBuffer(((shadow ? _shadowPollyType[b] : _mainPollyType[b])== PrimitiveTopology.LineList)? piece .LineIndices: piece.Indices, IndexFormat.UInt32);
				cl.SetGraphicsResourceSet(0, shadow ? _shadowRS[b] : _mainRS[b]);
				cl.DrawIndexed(((shadow ? _shadowPollyType[b] : _mainPollyType[b]) == PrimitiveTopology.LineList) ? piece.LineIndexCount : piece.IndexCount);
			}
		}

		public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
		{
			return RenderOrderKey.Create(RenderOrderOffset.Value, BoundingBox.DistanceFromPoint((Vector3)Entity.GlobalPointToLocal((Vector3f)cameraPosition, false)));
		}


		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{

		}
		public override void OnLoaded()
		{
            if (Gd is null)
            {
                return;
            }
            _wvpBuffer = Engine.RenderManager.Gd.ResourceFactory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
			AddDisposable(_wvpBuffer);
		}
		public override void OnChanged()
		{
		}

        public override void Dispose()
        {
            try
            {
                ClearPipeLine();
            }
            catch
            {
            }
            base.Dispose();
        }
        public MeshRender(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MeshRender()
		{
		}
	}
}
