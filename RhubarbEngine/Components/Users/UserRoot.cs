using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using System.Numerics;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using Veldrid;
using Veldrid.Sdl2;
using System.Runtime.InteropServices;
using g3;
namespace RhubarbEngine.Components.Users
{
    [Category(new string[] { "Users" })]
    public class UserRoot : Component
    {

        private float _moveSpeed = 10.0f;

        public SyncRef<Entity> Head;

        public SyncRef<Entity> LeftHand;
        public SyncRef<Entity> RightHand;

        public SyncRef<User> user;

        public Driver<Vector3f> posDriver;
        public Driver<Quaternionf> rotDriver;
        public Driver<Vector3f> scaleDriver;

        public Matrix4x4 Viewpos => entity.globalTrans();

        public Matrix4x4 Headpos => Head.target?.globalTrans()?? entity.globalTrans();

        public override void buildSyncObjs(bool newRefIds)
        {
            Head = new SyncRef<Entity>(this, newRefIds);
            LeftHand = new SyncRef<Entity>(this, newRefIds);
            RightHand = new SyncRef<Entity>(this, newRefIds);
            user = new SyncRef<User>(this, newRefIds);
            posDriver = new Driver<Vector3f>(this, newRefIds);
            rotDriver = new Driver<Quaternionf>(this, newRefIds);
            scaleDriver = new Driver<Vector3f>(this, newRefIds);

        }

        public override void onLoaded()
        {
            entity.persistence.value = false;
            logger.Log("Loaded Char");
        }

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
                return;
            };
            if (world.localUser == user.target)
            {
                if (input.mainWindows.GetKeyDown(Key.F7))
                {
                    Console.WriteLine(entity.globalPos());
                }
                if (input.isKeyboardinuse) return;
                float deltaSeconds = (float)world.worldManager.engine.platformInfo.deltaSeconds;
                float sprintFactor = world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ControlLeft) || engine.inputManager.PrimaryPress()
                   ? 0.1f
                   : world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ShiftLeft)
                   ? 2.5f
                     : 1f;
                Vector3 motionDir = Vector3.Zero;
                var leftvraix = engine.inputManager.Axis(Input.Creality.Left);
                var Rightvraix = engine.inputManager.Axis(Input.Creality.Right);
                motionDir -= (LeftHand.target.rotation.value.AxisZ * leftvraix.y).ToSystemNumrics();
                motionDir -= (RightHand.target.rotation.value.AxisZ * Rightvraix.y).ToSystemNumrics();

                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(leftvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
                lookRotation *= Quaternion.CreateFromYawPitchRoll(Rightvraix.x * -5f * deltaSeconds, 0.0f, 0.0f);
                float e = (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.X)) ? 0 : 1;
                e += (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.Z)) ? 0 : -1;
                lookRotation *= Quaternion.CreateFromYawPitchRoll(e * -5f * deltaSeconds, 0.0f, 0.0f);

                var temp = world.userRoot.Head.target.rotation.value * Vector3f.AxisZ;
                var HeadFacingDirection = new Vector3f(temp.x, 0, temp.z).Normalized;
                Quaternionf looke = Quaternionf.FromTo(Vector3f.AxisZ,HeadFacingDirection);
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.A))
                {
                    motionDir += -looke.AxisX.ToSystemNumrics();
                }
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.D))
                {
                    motionDir += looke.AxisX.ToSystemNumrics();
                }
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.W))
                {
                    motionDir += -looke.AxisZ.ToSystemNumrics();
                }
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.S))
                {
                    motionDir += looke.AxisZ.ToSystemNumrics();
                }
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.Q))
                {
                    motionDir += -Vector3.UnitY;
                }
                if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.E))
                {
                    motionDir += Vector3.UnitY;
                }
                if (motionDir != Vector3.Zero || lookRotation != default)
                {

                    motionDir = Vector3.Transform(motionDir, lookRotation);


                    Matrix4x4 addTo = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(lookRotation) * Matrix4x4.CreateTranslation(motionDir * _moveSpeed * sprintFactor * deltaSeconds);

                    var newtraans = addTo * entity.globalTrans();
                    entity.setGlobalTrans(newtraans);


                    var userpos = world.localUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserPos");
                    var userrot = world.localUser.FindOrCreateUserStream<SyncStream<Quaternionf>>("UserRot");
                    var userscale = world.localUser.FindOrCreateUserStream<SyncStream<Vector3f>>("UserScale");
                    Matrix4x4.Decompose(entity.globalTrans(), out Vector3 scale, out Quaternion rotation, out Vector3 translation);

                    userpos.value = new Vector3f(translation.X, translation.Y, translation.Z);
                    userrot.value = new Quaternionf(rotation.X, rotation.Y, rotation.Z, rotation.W);
                    userscale.value = new Vector3f(scale.X, scale.Y, scale.Z);
                    engine.worldManager.privateOverlay.userRoot.entity.setGlobalTrans(entity.globalTrans());
                }
            }
            else
            {
                if (user.target != null)
                {
                    var temp = user.target;
                    var userpos = temp.FindUserStream<SyncStream<Vector3f>>("UserPos");
                    var userrot = temp.FindUserStream<SyncStream<Quaternionf>>("UserRot");
                    var userscale = temp.FindUserStream<SyncStream<Vector3f>>("UserScale");
                    try
                    {
                        Matrix4x4 value = Matrix4x4.CreateScale(userscale.value.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(userrot.value.ToSystemNumric()) * Matrix4x4.CreateTranslation(userpos.value.ToSystemNumrics());
                        entity.setGlobalTrans(value);
                    }
                    catch
                    {

                    }

                }
            }
        }
        public UserRoot(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public UserRoot()
        {
        }
    }
}
