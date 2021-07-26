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
    public class ConeCollider : Collider
    {
        public Sync<double> radius;
        public Sync<double> height;

        public override void buildSyncObjs(bool newRefIds)
        {
            // Change the default values I guess

            base.buildSyncObjs(newRefIds);
            radius = new Sync<double>(this, newRefIds);
            radius.value = 0.5;
            radius.Changed += UpdateChange;

            height = new Sync<double>(this, newRefIds);
            height.value = 1.0;
            height.Changed += UpdateChange;
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
            startShape(new ConeShape(radius.value, height.value));
        }

        public ConeCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ConeCollider()
        {
        }
    }
}
