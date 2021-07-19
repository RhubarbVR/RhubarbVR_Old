using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class RevolveMesh : ProceduralMesh
    {
        private readonly Curve3Axis3RevolveGenerator _generator = new Curve3Axis3RevolveGenerator();

        public SyncValueList<Vector3d> Curve;
        
        public Sync<int> Slices;
        public Sync<bool> Capped;
        public Sync<bool> NoSharedVertices;
        

        public override void buildSyncObjs(bool newRefIds)
        {
            Curve = new SyncValueList<Vector3d>(this, newRefIds);
            Curve.Add().value = new Vector3d(1.0f, 0.25f, 0.0f);
            Curve.Add().value = new Vector3d(1.25f, 0.0f, 0.0f);
            Curve.Add().value = new Vector3d(1.0f, -0.25f, 0.0f);
            Curve.Add().value = new Vector3d(0.75f, 0.0f, 0.0f);
            
            Slices = new Sync<int>(this, newRefIds);
            Slices.value = 16;

            Capped = new Sync<bool>(this, newRefIds);
            Capped.value = false;
            NoSharedVertices = new Sync<bool>(this, newRefIds);

            onChanged();
        }
        public override void onChanged()
        {
            
            Vector3d[] tempArray = new Vector3d[Curve.Count];
            for (int i=0; i < Curve.Count; i++)
            {
                tempArray[i] = Curve[i].value;
            }
            _generator.Curve = tempArray;

            _generator.Slices = Slices.value;
            _generator.Capped = Capped.value;
            _generator.NoSharedVertices = NoSharedVertices.value;
            updateMesh();
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        private void updateMesh()
        {
            RMesh tempMesh = new RMesh(_generator.Generate().MakeDMesh());
            tempMesh.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(tempMesh);
        }

        public RevolveMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public RevolveMesh()
        {
        }
    }
}
