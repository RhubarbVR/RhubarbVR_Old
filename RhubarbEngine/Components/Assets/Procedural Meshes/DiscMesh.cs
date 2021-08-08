using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class DiscMesh : ProceduralMesh
    {
        private readonly TrivialDiscGenerator _generator = new TrivialDiscGenerator();

        public Sync<float> Radius;
        public Sync<float> StartAngleDeg;
        public Sync<float> EndAngleDeg;
        public Sync<int> Slices;

        public override void buildSyncObjs(bool newRefIds)
        {
            Radius = new Sync<float>(this, newRefIds);
            Radius.value = 1.0f;

            StartAngleDeg = new Sync<float>(this, newRefIds);
            StartAngleDeg.value = 0.0f;

            EndAngleDeg = new Sync<float>(this, newRefIds);
            EndAngleDeg.value = 360.0f;

            Slices = new Sync<int>(this, newRefIds);
            Slices.value = 32;
        }

        public override void onChanged()
        {
            updateMesh();
        }

        private void updateMesh()
        {
            _generator.Radius = Radius.value;
            _generator.StartAngleDeg = StartAngleDeg.value;
            _generator.EndAngleDeg = EndAngleDeg.value;
            _generator.Slices = Slices.value;
            MeshGenerator newmesh = _generator.Generate();
            RMesh kite = new RMesh(newmesh.MakeDMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite, true);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public DiscMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public DiscMesh()
        {
        }
    }
}

   