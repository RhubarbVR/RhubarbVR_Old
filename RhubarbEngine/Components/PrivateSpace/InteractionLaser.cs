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
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using BulletSharp.Math;
using g3;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.PrivateSpace
{
    public class InteractionLaser : Component
    {
        public Sync<InteractionSource> source;

        public Sync<Vector3f> rayderection;

        public Sync<float> distances;

        public Driver<Quaternionf> rotation;

        public override void OnAttach()
        {
            base.OnAttach();
            if (!world.userspace) return;
            rotation.setDriveTarget(entity.rotation);
        }


        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            source.value = InteractionSource.HeadLaser;
            rayderection = new Sync<Vector3f>(this, newRefIds);
            rayderection.value = -Vector3f.AxisZ;
            distances = new Sync<float>(this, newRefIds);
            distances.value = 25f;
            rotation = new Driver<Quaternionf>(this, newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (!world.userspace) return;
            if((engine.outputType == VirtualReality.OutputType.Screen) && source.value != InteractionSource.HeadLaser)
            {
                return;
            }
            if ((engine.outputType != VirtualReality.OutputType.Screen) && source.value == InteractionSource.HeadLaser)
            {
                return;
            }
            try
            {
                if(source.value == InteractionSource.HeadLaser)
                {
                    var mousepos = engine.inputManager.mainWindows.MousePosition;
                    var size = new System.Numerics.Vector2(engine.windowManager.mainWindow.width, engine.windowManager.mainWindow.height);
                    float x = 2.0f * mousepos.X / size.X - 1.0f;
                    float y = 2.0f * mousepos.Y / size.Y - 1.0f;
                    float ar = size.X / size.Y;
                    float tan = (float)Math.Tan(engine.settingsObject.RenderSettings.DesktopRenderSettings.fov * Math.PI/ 360);
                    Vector3f vectforward = new Vector3f(-x * tan * ar, y * tan, 1);
                    Vector3f vectup = new Vector3f(0, 1, 0);
                    entity.rotation.value = Quaternionf.LookRotation(vectforward, vectup);
                }
                System.Numerics.Matrix4x4.Decompose((System.Numerics.Matrix4x4.CreateTranslation((rayderection.value * distances.value).ToSystemNumrics()) * entity.globalTrans()), out System.Numerics.Vector3 vs, out System.Numerics.Quaternion vr, out System.Numerics.Vector3 val);
                System.Numerics.Matrix4x4.Decompose(entity.globalTrans(), out System.Numerics.Vector3 vsg, out System.Numerics.Quaternion vrg, out System.Numerics.Vector3 global);
                Vector3 sourcse = new Vector3(global.X, global.Y, global.Z);
                Vector3 destination = new Vector3(val.X, val.Y, val.Z);
                switch (source.value)
                {
                    case InteractionSource.LeftLaser:
                        input.LeftLaser.UpdateLaserPos(sourcse, destination);
                        break;
                    case InteractionSource.RightLaser:
                        input.RightLaser.UpdateLaserPos(sourcse, destination);
                        break;
                    case InteractionSource.HeadLaser:
                        input.RightLaser.UpdateLaserPos(sourcse, destination);
                        break;
                    default:
                        break;
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
