using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;

namespace RhubarbEngine.Render
{
    public abstract class Renderable : IDisposable
    {
        public abstract void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl);
        public abstract void Render(GraphicsDevice gd, CommandList cl, RenderPasses renderPass);
        public abstract void CreateDeviceObjects(GraphicsDevice gd, CommandList cl);
        public abstract void DestroyDeviceObjects();
        public abstract RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition);
        public virtual RenderPasses RenderPasses => RenderPasses.Standard;

        public void Dispose()
        {
            DestroyDeviceObjects();
        }
    }

    public abstract class CullRenderable : Renderable
    {
        public bool Cull(ref RhubarbEngine.Utilities.BoundingFrustum visibleFrustum)
        {
            return visibleFrustum.Contains(BoundingBox) == ContainmentType.Disjoint;
        }

        public abstract BoundingBox BoundingBox { get; }
    }
}
