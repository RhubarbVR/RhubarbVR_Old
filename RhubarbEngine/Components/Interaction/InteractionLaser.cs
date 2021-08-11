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

namespace RhubarbEngine.Components.Interaction
{

    [Category(new string[] { "Interaction" })]
    public class InteractionLaser : Component
    {
        public Sync<InteractionSource> source;

        public Sync<Vector3f> rayderection;

        public Sync<float> distances;

        public SyncRef<User> user;

        public SyncRef<CylinderMesh> mesh;

        public Driver<float> meshDriver;

        public override void OnAttach()
        {
            base.OnAttach();
            user.target = world.localUser;
            if (world.userspace) return;
            var (e, m) = MeshHelper.AddMesh<CylinderMesh>(entity);
            e.rotation.value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            meshDriver.target = m.Height;
            m.BaseRadius.value = 0.005f;
            m.TopRadius.value = 0.01f;
            mesh.target = m;
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            source = new Sync<InteractionSource>(this, newRefIds);
            source.value = InteractionSource.HeadLaser;
            rayderection = new Sync<Vector3f>(this, newRefIds);
            rayderection.value = -Vector3f.AxisZ;
            distances = new Sync<float>(this, newRefIds);
            distances.value = 10f;
            user = new SyncRef<User>(this, newRefIds);
            meshDriver = new Driver<float>(this, newRefIds);
            mesh = new SyncRef<CylinderMesh>(this, newRefIds);
        }

        private static float desklength = 0.1f;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (world.userspace) return;
            if (world.localUser != user.target) return;
            if ((engine.outputType != VirtualReality.OutputType.Screen) && source.value == InteractionSource.HeadLaser)
            {
                if (meshDriver.target != null)
                {
                    meshDriver.Drivevalue = 0;
                }
                return;
            }
            try
            {
                if(source.value == InteractionSource.HeadLaser)
                {
                    mesh.target.BaseRadius.value = 0.005f / 10;
                    mesh.target.TopRadius.value = 0.01f / 10;
                }
                System.Numerics.Matrix4x4.Decompose((System.Numerics.Matrix4x4.CreateTranslation((rayderection.value * distances.value).ToSystemNumrics()) * entity.globalTrans()), out System.Numerics.Vector3 vs, out System.Numerics.Quaternion vr, out System.Numerics.Vector3 val);
                System.Numerics.Matrix4x4.Decompose(entity.globalTrans(), out System.Numerics.Vector3 vsg, out System.Numerics.Quaternion vrg, out System.Numerics.Vector3 global);
                Vector3 sourcse = new Vector3(global.X, global.Y, global.Z);
                Vector3 destination = new Vector3(val.X, val.Y, val.Z);
                if (!HitTest(sourcse, destination, world.worldManager.privateOverlay))
                {
                    if (!HitTest(sourcse, destination, world))
                    {
                        if (meshDriver.target != null)
                        {
                            meshDriver.Drivevalue = (source.value == InteractionSource.HeadLaser) ? desklength : distances.value;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                logger.Log("Error With Interaction :" + e.ToString(), true);
            }

        }

        public bool HitTest(Vector3 sourcse, Vector3 destination, World.World eworld)
        {
            using (var cb = new ClosestRayResultCallback(ref sourcse, ref destination))
            {
                eworld.physicsWorld.RayTest(sourcse, destination, cb);
                if (cb.HasHit)
                {
                    if (meshDriver.target != null)
                    {
                        meshDriver.Drivevalue = (source.value == InteractionSource.HeadLaser) ? desklength : (float)Vector3.Distance(cb.HitPointWorld, sourcse);
                    }
                    Type type = cb.CollisionObject.UserObject.GetType();
                    if (type == typeof(InputPlane))
                    {
                        return ProossesInputPlane(cb);
                    }
                    else if (type == typeof(MeshInputPlane))
                    {
                        return ProossesMeshInputPlane(cb);
                    }
                }
                return false;
            }
        }

        public static Vector2f getUVPosOnTry(Vector3d p1, Vector2f p1uv, Vector3d p2, Vector2f p2uv, Vector3d p3, Vector2f p3uv, Vector3d point)
        {
            var f1 = p1 - point;
            var f2 = p2 - point;
            var f3 = p3 - point;
            var a = Vector3d.Cross(p1 - p2, p1 - p3).magnitude;
            var a1 = Vector3d.Cross(f2, f3).magnitude / a; 
            var a2 = Vector3d.Cross(f3, f1).magnitude / a; 
            var a3 = Vector3d.Cross(f1, f2).magnitude / a; 
            var uv = (p1uv * (float)a1) + (p2uv * (float)a2) + (p3uv * (float)a3);
            return uv;
        }

        public static Vector3d getNearestPosOnTry(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d point)
        {

            return new Vector3d();
        }

        private bool ProossesMeshInputPlane(ClosestRayResultCallback cb)
        {
            try
            {
                var inputPlane = ((MeshInputPlane)cb.CollisionObject.UserObject);
                System.Numerics.Matrix4x4.Decompose(inputPlane.entity.globalTrans(), out System.Numerics.Vector3 scale, out System.Numerics.Quaternion rotation, out System.Numerics.Vector3 translation);
                var pixsize = inputPlane.pixelSize.value;

                var hit = cb.HitPointWorld;
                var hitnormal = cb.HitNormalWorld;

                var stepone = ((hit - new Vector3(translation.X, translation.Y, translation.Z)) / new Vector3(scale.X, scale.Y, scale.Z));
                var steptwo = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3((float)stepone.X, (float)stepone.Y, (float)stepone.Z));
                var stepthree = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateFromQuaternion(System.Numerics.Quaternion.Inverse(rotation));
                var stepfour = (steptwo * stepthree);
                System.Numerics.Matrix4x4.Decompose(stepfour, out System.Numerics.Vector3 scsdale, out System.Numerics.Quaternion rotatdsion, out System.Numerics.Vector3 trans);
                
                if(inputPlane.mesh.Asset != null)
                {
                    var hittry = inputPlane.mesh.Asset.meshes[0].InsideTry(trans);
                    var tryangle = inputPlane.mesh.Asset.meshes[0].GetTriangle(hittry);
                    var mesh = inputPlane.mesh.Asset.meshes[0];
                    var p1 = mesh.GetVertexAll(tryangle.a);
                    var p2 = mesh.GetVertexAll(tryangle.b);
                    var p3 = mesh.GetVertexAll(tryangle.c);

                    var uvpos = getUVPosOnTry(p1.v, p1.uv, p2.v, p2.uv, p3.v, p3.uv,new Vector3d(trans.X, trans.Y, trans.Z));

                    var posnopixs = new Vector2f(uvpos.x, (-uvpos.y)+1);
                    var pospix = posnopixs * new Vector2f(pixsize.x, pixsize.y);
                    var pos = new System.Numerics.Vector2(pospix.x, pospix.y);
                    inputPlane.updatePos(pos, source.value);

                    if (HasClicked())
                    {
                        switch (source.value)
                        {
                            case InteractionSource.None:
                                break;
                            case InteractionSource.LeftLaser:
                                LeftLaser();
                                break;
                            case InteractionSource.LeftFinger:
                                break;
                            case InteractionSource.RightLaser:
                                RightLaser();
                                break;
                            case InteractionSource.RightFinger:
                                break;
                            case InteractionSource.HeadLaser:
                                break;
                            case InteractionSource.HeadFinger:
                                break;
                            default:
                                break;
                        }
                        inputPlane.Click(pos, source.value);
                    }
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }



        private bool ProossesInputPlane(ClosestRayResultCallback cb)
        {
            try
            {
                var inputPlane = ((InputPlane)cb.CollisionObject.UserObject);
                System.Numerics.Matrix4x4.Decompose(inputPlane.entity.globalTrans(), out System.Numerics.Vector3 scale, out System.Numerics.Quaternion rotation, out System.Numerics.Vector3 translation);
                var size = inputPlane.size.value;
                var pixsize = inputPlane.pixelSize.value;

                var hit = cb.HitPointWorld;
                var hitnormal = cb.HitNormalWorld;

                var stepone = ((hit - new Vector3(translation.X, translation.Y, translation.Z)) / new Vector3(scale.X, scale.Y, scale.Z));
                var steptwo = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3((float)stepone.X, (float)stepone.Y, (float)stepone.Z));
                var stepthree = System.Numerics.Matrix4x4.CreateScale(1) * System.Numerics.Matrix4x4.CreateFromQuaternion(System.Numerics.Quaternion.Inverse(rotation));
                var stepfour = (steptwo * stepthree);
                System.Numerics.Matrix4x4.Decompose(stepfour, out System.Numerics.Vector3 scsdale, out System.Numerics.Quaternion rotatdsion, out System.Numerics.Vector3 trans);
                var nonescaleedpos = new Vector2f(trans.X, -trans.Z);
                var posnopixs = ((nonescaleedpos * (1 / size)) / 2) + 0.5f;

                var pospix = posnopixs * new Vector2f(pixsize.x, pixsize.y);

                var pos = new System.Numerics.Vector2(pospix.x, pospix.y);
                inputPlane.updatePos(pos, source.value);
                if (HasClicked())
                {
                    switch (source.value)
                    {
                        case InteractionSource.None:
                            break;
                        case InteractionSource.LeftLaser:
                            LeftLaser();
                            break;
                        case InteractionSource.LeftFinger:
                            break;
                        case InteractionSource.RightLaser:
                            RightLaser();
                            break;
                        case InteractionSource.RightFinger:
                            break;
                        case InteractionSource.HeadLaser:
                            break;
                        case InteractionSource.HeadFinger:
                            break;
                        default:
                            break;
                    }
                    inputPlane.Click(pos, source.value);
                }
                return true;
            }
            catch
            {
            }
            return false;
        }


        private void RightLaser()
        {
            var e = Input.Creality.Right;
            if (input.SecondaryPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Right);
            }
            if (input.PrimaryPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Left);
            }
        }
        private void LeftLaser()
        {
            var e = Input.Creality.Left;
            if (input.SecondaryPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Right);
            }
            if (input.PrimaryPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Left);
            }
        }

            public bool HasClicked()
        {
            switch (source.value)
            {
                case InteractionSource.None:
                    break;
                case InteractionSource.LeftLaser:
                    return input.TriggerTouching(Input.Creality.Left) | input.SecondaryPress(Input.Creality.Left) | input.PrimaryPress(Input.Creality.Left);
                    break;
                case InteractionSource.LeftFinger:
                    break;
                case InteractionSource.RightLaser:
                    return input.TriggerTouching(Input.Creality.Right) | input.SecondaryPress(Input.Creality.Right) | input.PrimaryPress(Input.Creality.Right);
                    break;
                case InteractionSource.RightFinger:
                    break;
                case InteractionSource.HeadLaser:
                    return engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Right)| engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Left) | engine.inputManager.mainWindows.GetMouseButtonDown(MouseButton.Middle);
                    break;
                case InteractionSource.HeadFinger:
                    break;
                default:
                    break;
            }
            return false;
        }

        public InteractionLaser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public InteractionLaser()
        {
        }
    }
}
