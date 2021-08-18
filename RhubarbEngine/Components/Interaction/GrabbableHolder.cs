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
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using Veldrid;

namespace RhubarbEngine.Components.Interaction
{
    public class GrabbableHolder : Component
    {
        public SyncRef<Entity> holder;

        public SyncRef<InteractionLaser> laser;

        bool gripping = false;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            if (laser.target == null) return;
            if (holder.target == null) return;
            if (laser.target.user.target != world.localUser) return;
            if (onGriping() != gripping)
            {
                gripping = !gripping;
                if (!gripping)
                {
                    foreach (var child in holder.target._children)
                    {
                        foreach (var grab in child.getAllComponents<Grabbable>())
                        {
                            grab.Drop();
                        }
                    }
                }
                else
                {
                    world.lastHolder = this;
                }
            }
        }

        private bool onGriping()
        {
                switch (laser.target?.source?.value?? InteractionSource.None)
                {
                    case InteractionSource.None:
                        break;
                    case InteractionSource.LeftLaser:
                    return input.GrabPress(Input.Creality.Left);
                    break;
                    case InteractionSource.LeftFinger:
                        break;
                    case InteractionSource.RightLaser:
                    return input.GrabPress(Input.Creality.Right);
                        break;
                    case InteractionSource.RightFinger:
                        break;
                    case InteractionSource.HeadLaser:
                        return engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Right);
                        break;
                    case InteractionSource.HeadFinger:
                        break;
                    default:
                        break;
                }
                return false;
        }

        public override void OnAttach()
        {
            base.OnAttach();
            holder.target = entity.addChild("Holder");
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            holder = new SyncRef<Entity>(this, newRefIds);
            laser = new SyncRef<InteractionLaser>(this, newRefIds);
        }

        public GrabbableHolder(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public GrabbableHolder()
        {
        }
    }
}
