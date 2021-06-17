using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

/*
 *  For the love of all that is sacred and holy what in the creator's green world
 *  is this and how does it differ from the Cylinder we already have?
 */
namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class GenCylMesh : ProceduralMesh
    {
        private TubeGenerator genCylGen = new TubeGenerator();

        public Sync<Polygon2d> Polygon;
        public SyncValueList<Vector3d> Vertices;
        public Sync<Vector2d> CapCenter;
        public Sync<Frame3f> Frame;

        public Sync<bool> Capped;
        public Sync<bool> NoSharedVertices;
        public Sync<bool> OverrideCapCenter;
        public Sync<bool> ClosedLoop;
        public Sync<int> startCapCenterIndex;
        public Sync<int> endCapCenterIndex;

        public override void buildSyncObjs(bool newRefIds)
        {
            Polygon = new Sync<Polygon2d>(this, newRefIds);
            Vertices = new SyncValueList<Vector3d>(this, newRefIds);

            CapCenter = new Sync<Vector2d>(this, newRefIds);
            CapCenter.value = Vector2d.Zero;

            Frame = new Sync<Frame3f>(this, newRefIds);
            Frame.value = Frame3f.Identity;

            Capped = new Sync<bool>(this, newRefIds);
            Capped.value = true;

            NoSharedVertices = new Sync<bool>(this, newRefIds);
            NoSharedVertices.value = true;

            OverrideCapCenter = new Sync<bool>(this, newRefIds);
            OverrideCapCenter.value = false;

            ClosedLoop = new Sync<bool>(this, newRefIds);
            ClosedLoop.value = false;

            startCapCenterIndex = new Sync<int>(this, newRefIds);
            startCapCenterIndex.value = -1;

            endCapCenterIndex = new Sync<int>(this, newRefIds);
            endCapCenterIndex.value = -1;
        }

        public override void onChanged()
        {
            genCylGen.Polygon = Polygon.value;
            List<Vector3d> temp = new List<Vector3d>();
            foreach (Vector3d item in Vertices)
            {
                temp.Add(item);
            }
            genCylGen.Vertices = temp;
            genCylGen.CapCenter = CapCenter.value;
            genCylGen.Frame = Frame.value;
            genCylGen.Capped = Capped.value;
            genCylGen.NoSharedVertices = NoSharedVertices.value;
            genCylGen.OverrideCapCenter = OverrideCapCenter.value;
            genCylGen.ClosedLoop = ClosedLoop.value;
            genCylGen.startCapCenterIndex = startCapCenterIndex.value;
            genCylGen.endCapCenterIndex = endCapCenterIndex.value;
            updateMesh();
        }

        private void updateMesh()
        {
            MeshGenerator newmesh = genCylGen.Generate();
            RMesh kite = new RMesh(newmesh.MakeSimpleMesh());
            kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
            load(kite);
        }
        public override void onLoaded()
        {
            updateMesh();
        }
        public GenCylMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public GenCylMesh()
        {
        }

    }
}
