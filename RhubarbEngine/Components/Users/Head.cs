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
using RhubarbEngine.Input;

namespace RhubarbEngine.Components.Users
{
    [Category(new string[] { "Users" })]
    public class Head : Component
    {
        public SyncRef<UserRoot> userroot;

        public Driver<Vector3f> posDriver;
        public Driver<Quaternionf> rotDriver;
        public Driver<Vector3f> scaleDriver;

        public override void OnAttach()
        {
            base.OnAttach();
            posDriver.setDriveTarget(entity.position);
            rotDriver.setDriveTarget(entity.rotation);
            scaleDriver.setDriveTarget(entity.scale);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (world.userspace)
            {
                Matrix4x4 val = engine.renderManager.vrContext.Headpos;
                entity.setLocalTrans(val);
                return;
            }
            if(userroot.target == null)
            {
                return;
            }
            if (userroot.target.user.target == world.localUser)
            {
                Matrix4x4 val = engine.renderManager.vrContext.Headpos;
                entity.setLocalTrans(val);
                var userpos = world.localUser.FindOrCreateUserStream<SyncStream<Vector3f>>($"HeadPos");
                var userrot = world.localUser.FindOrCreateUserStream<SyncStream<Quaternionf>>($"HeadRot");
                var userscale = world.localUser.FindOrCreateUserStream<SyncStream<Vector3f>>($"HeadScale");
                userpos.value = entity.position.value;
                userrot.value = entity.rotation.value;
                userscale.value = entity.scale.value;
            }
            else
            {
                if (userroot.target.user.target != null)
                {
                    var temp = userroot.target.user.target;
                    var userpos = temp.FindUserStream<SyncStream<Vector3f>>($"HeadPos");
                    var userrot = temp.FindUserStream<SyncStream<Quaternionf>>($"HeadRot");
                    var userscale = temp.FindUserStream<SyncStream<Vector3f>>($"HeadScale");

                    try
                    {
                        Matrix4x4 value = Matrix4x4.CreateScale(userscale.value.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(userrot.value.ToSystemNumric()) * Matrix4x4.CreateTranslation(userpos.value.ToSystemNumrics());
                        entity.setLocalTrans(value);
                    }
                    catch
                    {

                    }

                }
            }
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            userroot = new SyncRef<UserRoot>(this, newRefIds);
            posDriver = new Driver<Vector3f>(this, newRefIds);
            rotDriver = new Driver<Quaternionf>(this, newRefIds);
            scaleDriver = new Driver<Vector3f>(this, newRefIds);
        }

        public Head(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public Head()
        {
        }
    }
}