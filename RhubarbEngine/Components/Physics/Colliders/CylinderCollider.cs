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
using RNumerics;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;

namespace RhubarbEngine.Components.Physics.Colliders
{

    [Category(new string[] { "Physics/Colliders" })]
    public class CylinderCollider : Collider
    {
        public Sync<Vector3f> halfExtents;

        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);

            halfExtents = new Sync<Vector3f>(this, newRefIds)
            {
                Value = new Vector3f(0.5)
            };
            halfExtents.Changed += UpdateChange;
        }

        public void UpdateChange(IChangeable val)
        {
            BuildShape();
        }

        public override void OnLoaded()
        {
            base.OnLoaded();
            BuildShape();
        }
        public override void BuildShape()
        {
            StartShape(new CylinderShape(halfExtents.Value.x, halfExtents.Value.y, halfExtents.Value.z));
        }

        public CylinderCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public CylinderCollider()
        {
        }
    }
}
