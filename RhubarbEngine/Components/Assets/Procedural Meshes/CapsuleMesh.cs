using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class CapsuleMesh : ProceduralMesh
    {
        private readonly CapsuleGenerator _generator = new CapsuleGenerator();

        public Sync<int> Longitudes;
        public Sync<int> Latitudes;
        public Sync<int> Rings;
        public Sync<float> Depth;
        public Sync<float> Radius;
        public Sync<CapsuleGenerator.UvProfile> Profile;
        
        public override void buildSyncObjs(bool newRefIds)
        {
            Longitudes = new Sync<int>(this, newRefIds);
            Longitudes.value = 32;
            Latitudes = new Sync<int>(this, newRefIds);
            Latitudes.value = 16;
            Rings = new Sync<int>(this, newRefIds);
            Rings.value = 0;
            Depth = new Sync<float>(this, newRefIds);
            Depth.value = 1.0f;
            Radius = new Sync<float>(this, newRefIds);
            Radius.value = 0.5f;
            Profile = new Sync<CapsuleGenerator.UvProfile>(this, newRefIds);
            Profile.value = CapsuleGenerator.UvProfile.Aspect;
        }
        public override void onChanged()
        {
            _generator.Longitudes = Longitudes.value;
            _generator.Latitudes = Latitudes.value;
            _generator.Rings = Rings.value;
            _generator.Depth = Depth.value;
            _generator.Radius = Radius.value;
            _generator.Profile = Profile.value;
            
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = _generator.Generate();
            RMesh kite = new RMesh(newmesh.MakeDMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite, true);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public CapsuleMesh(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public CapsuleMesh()
        {
        }
    }
}
