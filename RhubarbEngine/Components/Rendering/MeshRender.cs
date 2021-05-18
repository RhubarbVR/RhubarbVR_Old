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

namespace RhubarbEngine.Components.Rendering
{
    [Category(new string[] { "Rendering" })]
    public class MeshRender : Renderable
    {
        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl) { 
        }
        public override void Render(GraphicsDevice gd, CommandList cl, RenderPasses renderPass) {
        }
        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl) {
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
        public MeshRender(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public MeshRender()
        {
        }
    }
}
