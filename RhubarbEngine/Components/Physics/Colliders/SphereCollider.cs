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
    public class SphereCollider : Collider
    {
        public Sync<double> radius;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            radius = new Sync<double>(this, newRefIds);
            radius.value = 0.5;
            radius.Changed += UpdateChange;
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
            startShape(new SphereShape(radius.value));
        }

        public SphereCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SphereCollider()
        {
        }
    }
}
