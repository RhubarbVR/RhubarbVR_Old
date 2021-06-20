using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class BoxMesh : ProceduralMesh
    {
        private readonly TrivialBox3Generator _generator = new TrivialBox3Generator();

        public Sync<Vector3d> Center;
        public Sync<Vector3d> AxisX;
        public Sync<Vector3d> AxisY;
        public Sync<Vector3d> AxisZ;
        public Sync<Vector3d> Extent;

        public Sync<bool> NoSharedVertices;

        public override void buildSyncObjs(bool newRefIds)
        {
            Center = new Sync<Vector3d>(this, newRefIds);
            Center.value = Vector3d.Zero;
            AxisX = new Sync<Vector3d>(this, newRefIds);
            AxisX.value = Vector3d.AxisX;
            AxisY = new Sync<Vector3d>(this, newRefIds);
            AxisY.value = Vector3d.AxisY;
            AxisZ = new Sync<Vector3d>(this, newRefIds);
            AxisZ.value = Vector3d.AxisZ;
            Extent = new Sync<Vector3d>(this, newRefIds);
            Extent.value = Vector3d.One * 2;
            NoSharedVertices = new Sync<bool>(this, newRefIds);
        }
        public override void onChanged()
        {
            _generator.Box.Center = Center.value;
            _generator.Box.AxisX = AxisX.value;
            _generator.Box.AxisY = AxisY.value;
            _generator.Box.AxisZ = AxisZ.value;
            _generator.Box.Extent = Extent.value;
            _generator.NoSharedVertices = NoSharedVertices.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = _generator.Generate();
            RMesh kite = new RMesh(newmesh.MakeSimpleMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public BoxMesh(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public BoxMesh()
        {
        }
    }
}
