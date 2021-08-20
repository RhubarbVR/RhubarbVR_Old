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
    public class Grabbable:Component, IPhysicsDisableder
    {
        public SyncRef<Entity> lastParent;

        public SyncRef<User> grabbingUser;

        public SyncRef<GrabbableHolder> grabbableHolder;

        public bool Grabbed => (grabbableHolder.target != null)&&(grabbingUser.target != null);
        
        Vector3f lastValue;
        Vector3f volas;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            grabbableHolder = new SyncRef<GrabbableHolder>(this, newRefIds);
            grabbableHolder.Changed += GrabbableHolder_Changed;
            grabbingUser = new SyncRef<User>(this, newRefIds);
            lastParent = new SyncRef<Entity>(this, newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            if (grabbingUser.target != world.localUser) return;
            volas = (((lastValue - entity.globalPos()) *(1/(float)engine.platformInfo.deltaSeconds)) + volas)/2;
            lastValue = entity.globalPos();
        }

        private void GrabbableHolder_Changed(IChangeable obj)
        {
            if (grabbableHolder.target == null)
            {
                entity.RemovePhysicsDisableder(this);
            }
            else
            {
                entity.AddPhysicsDisableder(this);

            }
        }

        public override void Dispose()
        {
            base.Dispose();
            entity.RemovePhysicsDisableder(this);
        }

        public void Drop()
        {
            if (world.localUser != grabbingUser.target) return;
            grabbingUser.target = null;
            entity.SetParent(lastParent.target);
            entity.SendDrop(grabbableHolder.target, true);
            grabbableHolder.target = null;
            foreach (var item in entity.getAllComponents<Collider>())
            {
                if (item.NoneStaticBody.value && (item.collisionObject != null))
                {
                    Console.WriteLine("Droped V" + volas);
                    item.collisionObject.LinearVelocity = new BulletSharp.Math.Vector3(-volas.x, -volas.y, -volas.z);
                    item.collisionObject.AngularVelocity = new BulletSharp.Math.Vector3(volas.x/10, volas.y/10, volas.z/10);
                }
            }
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
