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
using BulletSharp;
using BulletSharp.Math;
using g3;
namespace RhubarbEngine.Components.Interaction
{
    public enum InteractionSource
    {
        None,
        LeftLaser,
        LeftFinger,
        RightLaser,
        RightFinger,
        HeadLaser,
        HeadFinger,
    }
    [Category(new string[] { "Interaction" })]
    public class InteractionLaser: Component
    {
        public Sync<InteractionSource> source;

        public Sync<Vector3f> rayderection;

        public Sync<float> distances;

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            rayderection = new Sync<Vector3f>(this, newRefIds);
            rayderection.value = Vector3f.AxisX;
            distances = new Sync<float>(this, newRefIds);
            distances.value = 10f;
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            try
            {
                var val = (System.Numerics.Matrix4x4.CreateTranslation((rayderection.value * distances.value).ToSystemNumrics()) * entity.globalTrans()).Translation;

                Vector3f global = entity.globalPos();
               // Vector3 source = new Vector3(global.x, global.y, global.z);
               // Vector3 destination = new Vector3(val.X, val.Y, val.Z);
                Vector3 source = new Vector3(2, 0, 0);
                Vector3 destination = new Vector3(0);
                logger.Log("Source "+ source.ToString()+ " destination:"+ destination.ToString());
                
                using (var cb = new ClosestRayResultCallback(ref source, ref destination))
                {
                    cb.Flags = 0xFFFFFFFF;
                    world.physicsWorld.RayTest(source, destination, cb);
                    if (cb.HasHit)
                    {
                        logger.Log("Trains");
                    }
                    else
                    {
                        logger.Log("NoTrains");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log("Error With Interaction :" + e.ToString(), true);
            }

        }
        public InteractionLaser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public InteractionLaser()
        {
        }
    }
}
