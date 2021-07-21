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

namespace RhubarbEngine.Components.Physics.Colliders
{

    [Category(new string[] { "Physics/Colliders" })]
    public class BoxCollider : Collider
    {
        public Sync<Vector3f> boxHalfExtents;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            boxHalfExtents = new Sync<Vector3f>(this, newRefIds);
            boxHalfExtents.value = new Vector3f(0.5f);
            boxHalfExtents.Changed += UpdateChange;
        }

        public void UpdateChange(IChangeable val)
        {
            BuildShape();
        }

        public override void onLoaded()
        {
            base.onLoaded();
            BuildShape();
        }

        public override void BuildShape()
        {
            startShape(new BoxShape(new BulletSharp.Math.Vector3(boxHalfExtents.value.x, boxHalfExtents.value.y, boxHalfExtents.value.z)));
        }

        public BoxCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public BoxCollider()
        {
        }
    }
}
