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
    public enum LookAtPace
    {
        Root,
        Head,
        LeftController,
        RightController
    }

    [Category(new string[] { "Transform" })]
    public class LookAtUser : Component
    {
        public Driver<Quaternionf> driver;

        public Sync<Quaternionf> offset;

        public Sync<Vector3f> positionOffset;

        public Sync<LookAtPace> positionSource;

        public override void buildSyncObjs(bool newRefIds)
        {
            driver = new Driver<Quaternionf>(this, newRefIds);
            offset = new Sync<Quaternionf>(this, newRefIds);
            offset.value = Quaternionf.Identity;
            positionOffset = new Sync<Vector3f>(this, newRefIds);
            positionSource = new Sync<LookAtPace>(this, newRefIds);
            positionSource.value = LookAtPace.Head;
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (driver.Linked)
            {
                Vector3f? tagetPos;
                switch (positionSource.value)
                {
                    case LookAtPace.Root:
                        tagetPos = world.localUser.userroot.target?.entity.globalPos();
                        break;
                    case LookAtPace.Head:
                        tagetPos = world.localUser.userroot.target?.Head.target?.globalPos();
                        break;
                    case LookAtPace.LeftController:
                        tagetPos = world.localUser.userroot.target?.LeftHand.target?.globalPos();
                        break;
                    case LookAtPace.RightController:
                        tagetPos = world.localUser.userroot.target?.RightHand.target?.globalPos();
                        break;
                    default:
                        tagetPos = null;
                        break;
                }
                Vector3f tangent = (((tagetPos ?? Vector3f.AxisY) + positionOffset.value) - entity.globalPos());
                tangent.Normalize();
                Vector3f normal = Vector3f.AxisY;
                var newrot = Quaternionf.LookRotation(tangent, normal) * offset.value;
                driver.Drivevalue = entity.GlobalRotToLocal(newrot,false);
            }
            else
            {
                driver.target = entity.rotation;
            }
        }
        public LookAtUser(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public LookAtUser()
        {
        }
    }
}
