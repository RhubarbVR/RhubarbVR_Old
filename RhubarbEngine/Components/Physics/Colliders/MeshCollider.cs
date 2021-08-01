using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.Physics.Colliders
{

    [Category(new string[] { "Physics/Colliders" })]
    public class MeshCollider : Collider
    {
        public AssetRef<RMesh> mesh;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            mesh = new AssetRef<RMesh>(this, newRefIds);
            mesh.loadChange += Mesh_loadChange;
        }

        private void Mesh_loadChange(RMesh obj)
        {
            BuildShape();
        }

        public override void onLoaded()
        {
            base.onLoaded();
            BuildShape();
        }

        private void goNull()
        {
            buildCollissionObject(null);
        }

        public override void BuildShape()
        {
            if (mesh.Asset == null) { goNull(); return; };
            if (!mesh.target?.loaded??false) { goNull(); return; };
            TriangleIndexVertexArray stridingMeshInterface = new TriangleIndexVertexArray((ICollection<int>)mesh.Asset.meshes[0].TriangleIndices(), (ICollection<BulletSharp.Math.Vector3>)mesh.Asset.meshes[0].VertexPos().Select(v3 => new BEPUutilities.Vector3(v3.x, v3.y,v3.z)));
            BvhTriangleMeshShape trys = new BvhTriangleMeshShape(stridingMeshInterface, true);
            startShape(trys);
        }

        public MeshCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public MeshCollider()
        {
        }
    }
}
