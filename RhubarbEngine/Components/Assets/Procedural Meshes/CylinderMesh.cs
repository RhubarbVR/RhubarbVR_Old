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
    public class CylinderMesh : ProceduralMesh
    {
        private OpenCylinderGenerator cylGen = new OpenCylinderGenerator();

        public Sync<float> BaseRadius;
        public Sync<float> TopRadius;
        public Sync<float> Height;
        public Sync<float> StartAngleDeg;
        public Sync<float> EndAngleDeg;
        public Sync<int> Slices;
        public Sync<bool> NoSharedVertices;

        public override void buildSyncObjs(bool newRefIds)
        {
            BaseRadius = new Sync<float>(this, newRefIds);
            BaseRadius.value = 1.0f;

            TopRadius = new Sync<float>(this, newRefIds);
            TopRadius.value = 1.0f;

            Height = new Sync<float>(this, newRefIds);
            Height.value = 1.0f;

            StartAngleDeg = new Sync<float>(this, newRefIds);
            StartAngleDeg.value = 0.0f;

            EndAngleDeg = new Sync<float>(this, newRefIds);
            EndAngleDeg.value = 360.0f;

            Slices = new Sync<int>(this, newRefIds);
            Slices.value = 16;

            NoSharedVertices = new Sync<bool>(this, newRefIds);
            NoSharedVertices.value = false;
        }

        public override void onChanged()
        {
            cylGen.BaseRadius = BaseRadius.value;
            cylGen.TopRadius = TopRadius.value;
            cylGen.Height = Height.value;
            cylGen.StartAngleDeg = StartAngleDeg.value;
            cylGen.EndAngleDeg = EndAngleDeg.value;
            cylGen.Slices = Slices.value;
            cylGen.NoSharedVertices = NoSharedVertices.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = cylGen.Generate();
            RMesh kite = new RMesh(newmesh.MakeSimpleMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public CylinderMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public CylinderMesh()
        {
        }

    }


}
