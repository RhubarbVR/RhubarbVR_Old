﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using g3;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Managers;
using RhubarbEngine.World.ECS;
using Veldrid;

namespace RhubarbEngine.Input
{
    public class InteractionLaserSource
    {
        public Creality side;

        private InputManager inputManager;

        private Engine engine;

        private InputManager input => engine.inputManager;

        public InteractionLaserSource(Creality _side, InputManager _inputManager,Engine _engine)
        {
            side = _side;
            inputManager = _inputManager;
            engine = _engine;
        }
        private bool HasClicked()
        {
            if((engine.outputType == VirtualReality.OutputType.Screen)&&(side == Creality.Right))
            {
                return (engine.inputManager.mainWindows.GetMouseButton(MouseButton.Right)) | engine.inputManager.mainWindows.GetMouseButton(MouseButton.Left) | engine.inputManager.mainWindows.GetMouseButton(MouseButton.Middle);
            }
            switch (side)
            {
                case Creality.None:
                    break;
                case Creality.Left:
                    return input.TriggerTouching(Input.Creality.Left) | input.GrabPress(Input.Creality.Left) | input.PrimaryPress(Input.Creality.Left);
                case Creality.Right:
                    return input.TriggerTouching(Input.Creality.Right) | input.GrabPress(Input.Creality.Right) | input.PrimaryPress(Input.Creality.Right);
                default:
                    break;
            }
            return false;
        }

        private static Vector2f getUVPosOnTry(Vector3d p1, Vector2f p1uv, Vector3d p2, Vector2f p2uv, Vector3d p3, Vector2f p3uv, Vector3d point)
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

        private void RightLaser()
        {
            var e = Input.Creality.Right;
            if (input.GrabPress(e))
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
            if (input.GrabPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Right);
            }
            if (input.PrimaryPress(e))
            {
                input.mainWindows.FrameSnapshot.MouseClick(MouseButton.Left);
            }
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

                if (inputPlane.mesh.Asset != null)
                {
                    var hittry = inputPlane.mesh.Asset.meshes[0].InsideTry(trans);
                    var tryangle = inputPlane.mesh.Asset.meshes[0].GetTriangle(hittry);
                    var mesh = inputPlane.mesh.Asset.meshes[0];
                    var p1 = mesh.GetVertexAll(tryangle.a);
                    var p2 = mesh.GetVertexAll(tryangle.b);
                    var p3 = mesh.GetVertexAll(tryangle.c);

                    var uvpos = getUVPosOnTry(p1.v, p1.uv, p2.v, p2.uv, p3.v, p3.uv, new Vector3d(trans.X, trans.Y, trans.Z));

                    var posnopixs = new Vector2f(uvpos.x, (-uvpos.y) + 1);
                    var pospix = posnopixs * new Vector2f(pixsize.x, pixsize.y);
                    var pos = new System.Numerics.Vector2(pospix.x, pospix.y);

                    var source = InteractionSource.None;
                    if ((engine.outputType == VirtualReality.OutputType.Screen) && (side == Creality.Right))
                    {
                        source = InteractionSource.HeadLaser;
                    }
                    else
                    {
                        switch (side)
                        {
                            case Creality.Left:
                                source = InteractionSource.LeftLaser;
                                break;
                            case Creality.Right:
                                source = InteractionSource.RightLaser;
                                break;
                            default:
                                break;
                        }
                    }
                    inputPlane.updatePos(pos, source);

                    if (HasClicked())
                    {
                        switch (source)
                        {
                            case InteractionSource.LeftLaser:
                                LeftLaser();
                                break;
                            case InteractionSource.RightLaser:
                                RightLaser();
                                break;
                            default:
                                break;
                        }
                        inputPlane.Click(pos, source);
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

                var source = InteractionSource.None;
                if ((engine.outputType == VirtualReality.OutputType.Screen) && (side == Creality.Right))
                {
                    source = InteractionSource.HeadLaser;
                }
                else
                {
                    switch (side)
                    {
                        case Creality.Left:
                            source = InteractionSource.LeftLaser;
                            break;
                        case Creality.Right:
                            source = InteractionSource.RightLaser;
                            break;
                        default:
                            break;
                    }
                }
                inputPlane.updatePos(pos, source);
                if (HasClicked())
                {
                    switch (source)
                    {
                        case InteractionSource.LeftLaser:
                            LeftLaser();
                            break;
                        case InteractionSource.RightLaser:
                            RightLaser();
                            break;
                        default:
                            break;
                    }
                    inputPlane.Click(pos, source);
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        private bool ProssesCollider(ClosestRayResultCallback cb)
        {
            Collider col = (Collider)cb.CollisionObject.UserObject;
            if (col == null)
            {
                return false;
            }
            Entity ent = col.entity;
            if (HasClicked())
            {
                ent.SendClick();
                var source = InteractionSource.None;
                if ((engine.outputType == VirtualReality.OutputType.Screen) && (side == Creality.Right))
                {
                    source = InteractionSource.HeadLaser;
                }
                else
                {
                    switch (side)
                    {
                        case Creality.Left:
                            source = InteractionSource.LeftLaser;
                            break;
                        case Creality.Right:
                            source = InteractionSource.RightLaser;
                            break;
                        default:
                            break;
                    }
                }
                switch (source)
                {
                    case InteractionSource.None:
                        break;
                    case InteractionSource.LeftLaser:
                        ent.SendTriggerTouching(input.TriggerTouching(Input.Creality.Left));
                        ent.SendSecondary(input.SecondaryPress(Input.Creality.Left));
                        ent.SendPrimary(input.PrimaryPress(Input.Creality.Left));
                        ent.SendGrip(col.world.LeftLaserGrabbableHolder, input.GrabPress(Input.Creality.Left));
                        break;
                    case InteractionSource.LeftFinger:
                        break;
                    case InteractionSource.RightLaser:
                        ent.SendTriggerTouching(input.TriggerTouching(Input.Creality.Right));
                        ent.SendSecondary(input.SecondaryPress(Input.Creality.Right));
                        ent.SendPrimary(input.PrimaryPress(Input.Creality.Right));
                        ent.SendGrip(col.world.RightLaserGrabbableHolder, input.GrabPress(Input.Creality.Right));
                        break;
                    case InteractionSource.RightFinger:
                        break;
                    case InteractionSource.HeadLaser:
                        ent.SendSecondary(input.mainWindows.GetMouseButton(MouseButton.Middle));
                        ent.SendPrimary(input.mainWindows.GetMouseButton(MouseButton.Left));
                        ent.SendGrip(col.world.HeadLaserGrabbableHolder, input.mainWindows.GetMouseButton(MouseButton.Right));
                        break;
                    case InteractionSource.HeadFinger:
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private void ProssecesHitPoint(Vector3d pos, Vector3d normal)
        {

        }

        private Vector3 sourcse;

        private Vector3 destination;

        public void UpdateLaserPos(Vector3 _sourcse,Vector3 _destination)
        {
            sourcse = _sourcse;
            destination = _destination;
        }

        private void ProsscesHit()
        {
            if (!HitTest(sourcse, destination, engine.worldManager.privateOverlay))
            {
                bool hittestbool = false;
                foreach (var item in engine.worldManager.worlds)
                {
                    if ((item.Focus == World.World.FocusLevel.Overlay) && !hittestbool)
                    {
                        hittestbool = HitTest(sourcse, destination, item);
                    }
                }
                if ((!HitTest(sourcse, destination, engine.worldManager.focusedWorld)) && !hittestbool)
                {
                    ProssecesHitPoint(Vector3d.Zero, Vector3d.Zero);
                }
            }
        }

        private bool HitTest(Vector3 sourcse, Vector3 destination, World.World eworld)
        {
            if (eworld == null) return false;
            using (var cb = new ClosestRayResultCallback(ref sourcse, ref destination))
            {
                eworld.physicsWorld.RayTest(sourcse, destination, cb);
                if (cb.HasHit)
                {
                    ProssecesHitPoint(new Vector3d(cb.HitPointWorld.X, cb.HitPointWorld.Y, cb.HitPointWorld.Z), new Vector3d(cb.HitNormalWorld.X, cb.HitNormalWorld.Y, cb.HitNormalWorld.Z));
                    Type type = cb.CollisionObject.UserObject.GetType();
                    if (type == typeof(InputPlane))
                    {
                        return ProossesInputPlane(cb);
                    }
                    else if (type == typeof(MeshInputPlane))
                    {
                        return ProossesMeshInputPlane(cb);
                    }
                    else if (typeof(Collider).IsAssignableFrom(type))
                    {
                        return ProssesCollider(cb);
                    }
                }
                return false;
            }
        }

        public void Update()
        {
            ProsscesHit();
        }

    }
}