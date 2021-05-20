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

namespace RhubarbEngine.Components.Transform
{
    [Category(new string[] { "Transform" })]
    public class Spinner : Component
    {
        public Driver<Quaternionf> driver;

        public Sync<Vector3f> speed;

        public Sync<Quaternionf> offset;

        public override void buildSyncObjs(bool newRefIds)
        {
            driver = new Driver<Quaternionf>(this, newRefIds);
            speed = new Sync<Vector3f>(this, newRefIds);
            offset = new Sync<Quaternionf>(this, newRefIds);
            speed.value = new Vector3f(1f,0f,0f);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            float deltaSeconds = (float)world.worldManager.engine.platformInfo.deltaSeconds;
            if (driver.Linked)
            {
                Matrix4x4 newval = entity.localTrans() * Matrix4x4.CreateFromQuaternion(offset.value.ToSystemNumric()) * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(speed.value.x* deltaSeconds, speed.value.y * deltaSeconds, speed.value.z * deltaSeconds));
                Matrix4x4.Decompose(newval, out Vector3 newscale, out Quaternion newrotation, out Vector3 newtranslation);
                driver.Drivevalue = new Quaternionf(newrotation.X, newrotation.Y, newrotation.Z, newrotation.W);
            }
            else
            {
                driver.target = entity.rotation;
            }
        }
        public Spinner(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public Spinner()
        {
        }
    }
}
