using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;

namespace RhubarbEngine.Render
{
    public class MeshPiece
    {
        public DeviceBuffer Positions { get; }
        public DeviceBuffer TexCoords { get; }
        public DeviceBuffer Indices { get; }
        public uint IndexCount { get; }

        public MeshPiece(DeviceBuffer positions, DeviceBuffer texCoords, DeviceBuffer indices)
        {
            Positions = positions;
            TexCoords = texCoords;
            Indices = indices;
            IndexCount = indices.SizeInBytes / sizeof(uint);
        }
    }

    public struct UBO
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 World;

        public UBO(Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Projection = projection;
            View = view;
            World = world;
        }
    }

    public abstract class Renderable : Component, IDisposable
    {
        public abstract void Render(GraphicsDevice gd, CommandList cl, UBO ubo);
        public abstract void RenderShadow(GraphicsDevice gd, CommandList cl, UBO ubo);

        public abstract RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition);

        public bool Cull(ref RhubarbEngine.Utilities.BoundingFrustum visibleFrustum,Matrix4x4 view)
        {
            return visibleFrustum.Contains(Veldrid.Utilities.BoundingBox.Transform(BoundingBox, entity.globalTrans() * view)) == ContainmentType.Disjoint;
        }

        public abstract BoundingBox BoundingBox { get; }

        public Renderable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Renderable()
        {
        }

        public void Dispose()
        {
        }
    }

}
