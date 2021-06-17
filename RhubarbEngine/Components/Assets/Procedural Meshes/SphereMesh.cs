using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class SphereMesh : ProceduralMesh
    {
        private readonly Sphere3Generator_NormalizedCube sphereGen = new Sphere3Generator_NormalizedCube();

        public Sync<double> Radius;
        public Sync<int> EdgeVertices;


        public Sync<bool> NoSharedVertices;

        public override void buildSyncObjs(bool newRefIds)
        {
            Radius = new Sync<double>(this, newRefIds);
            EdgeVertices = new Sync<int>(this, newRefIds);

            NoSharedVertices = new Sync<bool>(this, newRefIds);
        }
        public override void onChanged()
        {
            sphereGen.Radius = Radius.value;
            sphereGen.EdgeVertices = EdgeVertices.value;
            sphereGen.NoSharedVertices = NoSharedVertices.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = sphereGen.Generate();
            RMesh kite = new RMesh(newmesh.MakeSimpleMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public SphereMesh(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public SphereMesh()
        {
        }
    }
}
