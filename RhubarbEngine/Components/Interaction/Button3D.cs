using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics.Colliders;

//TODO: Button interacting with laser or finger press. work in progress none functional.

namespace RhubarbEngine.Components.Interaction
{
    [Category(new string[] { "Interaction" })]
    public class Button3D : Component
    {
        public SyncRef<Entity> ClickVisual;
        public Sync<Vector3d> ClickAxis;

        public Sync<bool> IsLaserClickable;

        public bool Click;
        private bool _hover;
        private float _pressDepth;
        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);
            ClickVisual = new SyncRef<Entity>(this, newRefIds);
            ClickAxis = new Sync<Vector3d>(this, newRefIds);
            IsLaserClickable = new Sync<bool>(this, newRefIds);
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);

        }
        public Button3D(){}
    }
}
