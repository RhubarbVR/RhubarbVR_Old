using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using BaseR;
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

namespace RhubarbEngine.Components.Rendering
{
    [Category(new string[] { "Rendering" })]
    public class MeshRender : Renderable
    {
        private const string vertexGlsl =
@"
#version 450

layout (set = 0, binding = 0) uniform WVP
{
    mat4 Proj;
    mat4 View;
    mat4 World;
};

layout (location = 0) in vec3 vsin_Position;
layout (location = 1) in vec2 vsin_UV;

layout (location = 0) out vec2 fsin_UV;

void main()
{
    gl_Position = Proj * View * World * vec4(vsin_Position, 1);
    fsin_UV = vsin_UV;
}
";
        private const string fragmentGlsl =
@"
#version 450

layout(set = 0, binding = 1) uniform texture2D Input;
layout(set = 0, binding = 2) uniform sampler InputSampler;

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = vec4(0.0, 0.0, 0.0, 0.0);
}
";

        private GraphicsDevice _gd;
        private List<IDisposable> _disposables = new List<IDisposable>();
        private List<MeshPiece> _meshPieces = new List<MeshPiece>();
        private Pipeline _pipeline;
        private DeviceBuffer _wvpBuffer;
        private Texture _texture;
        private TextureView _view;
        private ResourceSet _rs;
        private bool loaded;
        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl) { 
        }
        public override void Render(GraphicsDevice gd, CommandList cl, RenderPasses renderPass, UBO ubo) {
            if (!loaded)
            {
                return;
            }
            cl.UpdateBuffer(_wvpBuffer, 0, ubo);
            cl.SetPipeline(_pipeline);
            foreach (MeshPiece piece in _meshPieces)
            {
                cl.SetVertexBuffer(0, piece.Positions);
                cl.SetVertexBuffer(1, piece.TexCoords);
                cl.SetIndexBuffer(piece.Indices, IndexFormat.UInt32);
                cl.SetGraphicsResourceSet(0, _rs);
                cl.DrawIndexed(piece.IndexCount);
            }
        }
        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
        {
            Console.WriteLine("Created Device Objects");
            _gd = gd;
            ResourceFactory factory = gd.ResourceFactory;

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(vertexGlsl), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(fragmentGlsl), "main"));
            _disposables.Add(shaders[0]);
            _disposables.Add(shaders[1]);

            ResourceLayout rl = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WVP", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Input", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("InputSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _disposables.Add(rl);

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

            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { positionLayoutDesc, texCoordLayoutDesc }, new Shader[] { shaders[0], shaders[1] }),
                rl,
                world.worldManager.engine.renderManager.vrContext.LeftEyeFramebuffer.OutputDescription));
            _disposables.Add(_pipeline);

            _wvpBuffer = factory.CreateBuffer(new BufferDescription(64 * 3, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _disposables.Add(_wvpBuffer);

            _texture = new ImageSharpTexture("C:\\Rhubarb\\veldrid-master\\src\\Veldrid.VirtualReality.Sample\\cat\\cat_diff.png", true, true).CreateDeviceTexture(gd, factory);
            _view = factory.CreateTextureView(_texture);
            _disposables.Add(_texture);
            _disposables.Add(_view);

            _rs = factory.CreateResourceSet(new ResourceSetDescription(rl, _wvpBuffer, _view, gd.Aniso4xSampler));
            _disposables.Add(_rs);

            IMesh mesh = source.target.value;
            IList<Vector3d> Vertices = new List<Vector3d>();
            IList<Vector2f> UV = new List<Vector2f>();
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertices.Add(mesh.GetVertex(i));
                UV.Add(mesh.GetVertexUV(i));
            }
            DeviceBuffer positions = CreateDeviceBuffer(Vertices.Select(v3 => new Vector3((float)v3.x, (float)v3.y, (float)v3.z)).ToArray(), BufferUsage.VertexBuffer);
                DeviceBuffer texCoords = CreateDeviceBuffer(
                    UV.Select(v3 => new Vector2(v3.x, v3.y)).ToArray(),
                    BufferUsage.VertexBuffer);
                DeviceBuffer indices = CreateDeviceBuffer(mesh.RenderIndices().Select(v3 => (uint)v3).ToArray(), BufferUsage.IndexBuffer);

            _meshPieces.Add(new MeshPiece(positions, texCoords, indices));

            loaded = true;
        }


        public DeviceBuffer CreateDeviceBuffer<T>(IList<T> list, BufferUsage usage) where T : unmanaged
        {
            DeviceBuffer buffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * list.Count), usage));
            _disposables.Add(buffer);
            _gd.UpdateBuffer(buffer, 0, list.ToArray());
            return buffer;
        }
        public override void DestroyDeviceObjects() {
        }
        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition) {
            return RenderOrderKey.Create(0, 1);
        }

        public SyncRef<AssetProvider<IMesh>> source;

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new SyncRef<AssetProvider<IMesh>>(this, newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }
        public override void onLoaded()
        {
            if(source.target == null)
            {
                return;
            }
            CreateDeviceObjects(world.worldManager.engine.renderManager.gd, world.worldManager.engine.renderManager.windowCL);
        }
        public override void onChanged()
        {
            CreateDeviceObjects(world.worldManager.engine.renderManager.gd, world.worldManager.engine.renderManager.windowCL);
        }
        public MeshRender(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public MeshRender()
        {
        }
    }
}
