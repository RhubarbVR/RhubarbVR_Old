using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class BoxMesh : AssetProvider<RMesh>
    {
        private TrivialBox3Generator boxgen = new TrivialBox3Generator();

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
            boxgen.Box.Center = Center.value;
            boxgen.Box.AxisX = AxisX.value;
            boxgen.Box.AxisY = AxisY.value;
            boxgen.Box.AxisZ = AxisZ.value;
            boxgen.Box.Extent = Extent.value;
            boxgen.NoSharedVertices = NoSharedVertices.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = boxgen.Generate();
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
