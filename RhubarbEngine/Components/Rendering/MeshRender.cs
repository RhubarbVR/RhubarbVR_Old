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
    public class MeshRender : CullRenderable
    {

        public AssetRef<RMesh> Mesh;
        public SyncAssetRefList<RMaterial> Materials;
        public override BoundingBox BoundingBox => Mesh.Asset.boundingBox;

        public override void buildSyncObjs(bool newRefIds)
        {
            Mesh = new AssetRef<RMesh>(this, newRefIds);
            Materials = new SyncAssetRefList<RMaterial>(this, newRefIds);
            Mesh.loadChange += loadMesh;
            Materials.loadChange += loadMaterial;
        }

        private void loadMesh(RMesh mesh)
        {
            _meshPieces = mesh.meshPieces;
        }

        private void loadMaterial(RMaterial mit)
        {

        }

        private GraphicsDevice _gd;
        private List<MeshPiece> _meshPieces = new List<MeshPiece>();
        private Pipeline _pipeline;
        private DeviceBuffer _wvpBuffer;
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
            _gd = gd;
        }

        public override void DestroyDeviceObjects() {
        }
        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition) {
            return RenderOrderKey.Create(0, 1);
        }


        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {

        }
        public override void onLoaded()
        {

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
