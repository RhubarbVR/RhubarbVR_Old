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
        private readonly TubeGenerator _generator = new TubeGenerator();

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
        public override void OnAttach()
        {
            Vertices.Add(true).value = Vector3f.Zero;
            Vertices.Add(true).value = Vector3f.One;
            Vertices.Add(true).value = new Vector3f(10,10,10);

            logger.Log("Loaded");
        }
        public override void buildSyncObjs(bool newRefIds)
        {
            Polygon = new Sync<Polygon2d>(this, newRefIds);
            Polygon.value = Polygon2d.MakeCircle(1f, 1);
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
            _generator.CapCenter = CapCenter.value;
            _generator.Frame = Frame.value;
            _generator.Capped = Capped.value;
            _generator.NoSharedVertices = NoSharedVertices.value;
            _generator.OverrideCapCenter = OverrideCapCenter.value;
            _generator.ClosedLoop = ClosedLoop.value;
            _generator.startCapCenterIndex = startCapCenterIndex.value;
            _generator.endCapCenterIndex = endCapCenterIndex.value;
            updateMesh();
        }

        private void updateMesh()
        {
            _generator.Polygon = Polygon.value;
            List<Vector3d> temp = new List<Vector3d>();
            if(Vertices.Count <= 1)
            {
                temp.Add((Vertices.Count == 0) ? Vector3d .Zero: Vertices[0].value);
                temp.Add((Vertices.Count == 0) ? Vector3d.Zero : Vertices[0].value);
                logger.Log("e");
            }
            else
            {
                logger.Log("o");
                foreach (Vector3d item in Vertices)
                {
                    temp.Add(item);
                }
            }
            _generator.Vertices = temp;
            MeshGenerator newmesh = _generator.Generate();
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
