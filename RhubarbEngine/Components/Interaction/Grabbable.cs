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

namespace RhubarbEngine.Components.Interaction
{
    public class Grabbable:Component
    {
        public SyncRef<Entity> lastParent;

        public SyncRef<User> grabbingUser;

        public SyncRef<GrabbableHolder> grabbableHolder;

        public bool Grabbed => (grabbableHolder.target != null)&&(grabbingUser.target != null);

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            grabbableHolder = new SyncRef<GrabbableHolder>(this, newRefIds);
            grabbingUser = new SyncRef<User>(this, newRefIds);
            lastParent = new SyncRef<Entity>(this, newRefIds);

        }

        public void Drop()
        {
            if (world.localUser != grabbingUser.target) return;
            grabbingUser.target = null;
            entity.SetParent(lastParent.target);
            entity.SendDrop(grabbableHolder.target, true);
            grabbableHolder.target = null;
        }


        public override void onLoaded()
        {
            base.onLoaded();
            entity.onGrip += Entity_onGrip;
        }

        private void Entity_onGrip(GrabbableHolder obj)
        {
            if (grabbableHolder.target == obj) return;
            if (obj == null) return;
            if (obj.holder.target == null) return;
            if (!Grabbed)
            {
                lastParent.target = entity.parent.target;
            }
            entity.manager = world.localUser;
            grabbableHolder.target = obj;
            grabbingUser.target = world.localUser;
            entity.SetParent(obj.holder.target);
        }

        public void RemoteGrab()
        {
            if (world.lastHolder == null) return;
            Entity_onGrip(world.lastHolder);
        }

        public Grabbable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public Grabbable()
        {
        }
    }
}
