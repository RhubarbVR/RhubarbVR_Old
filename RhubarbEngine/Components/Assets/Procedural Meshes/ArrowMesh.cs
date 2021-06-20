using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class ArrowMesh : ProceduralMesh
    {
        private readonly Radial3DArrowGenerator _generator = new Radial3DArrowGenerator();

        public Sync<float> StickRadius;
        public Sync<float> StickLength;
        public Sync<float> HeadBaseRadius;
        public Sync<float> TipRadius;
        public Sync<float> HeadLength;
        public Sync<bool> DoubleSided;

        public override void buildSyncObjs(bool newRefIds)
        {
            StickRadius = new Sync<float>(this, newRefIds);
            StickRadius.value = 0.5f;

            StickLength = new Sync<float>(this, newRefIds);
            StickLength.value = 1.0f;

            HeadBaseRadius = new Sync<float>(this, newRefIds);
            HeadBaseRadius.value = 1.0f;

            TipRadius = new Sync<float>(this, newRefIds);
            TipRadius.value = 0.0f;

            HeadLength = new Sync<float>(this, newRefIds);
            HeadLength.value = 0.5f;
            
            DoubleSided = new Sync<bool>(this, newRefIds);
            DoubleSided.value = false;
        }
        public override void onChanged()
        {
            _generator.StickRadius = StickRadius.value;
            _generator.StickLength = StickLength.value;
            _generator.HeadBaseRadius = HeadBaseRadius.value;
            _generator.TipRadius = TipRadius.value;
            _generator.HeadLength = HeadLength.value;
            _generator.DoubleSided = DoubleSided.value;
            
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
        public ArrowMesh(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public ArrowMesh()
        {
        }
    }
}
