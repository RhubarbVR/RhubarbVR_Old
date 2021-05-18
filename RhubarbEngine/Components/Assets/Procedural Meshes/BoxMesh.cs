using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using BaseR;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

    [Category(new string[] { "Assets/Procedural Meshes" })]
    public class BoxMesh : AssetProvider<IMesh>
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
            Extent.value = Vector3d.One / 2;
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
            MeshGenerator newmesh = boxgen.Generate();
            load(newmesh.MakeSimpleMesh());
        }
        public BoxMesh(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public BoxMesh()
        {
        }
    }
}
