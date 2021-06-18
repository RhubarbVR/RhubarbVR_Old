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

namespace RhubarbEngine.Components.Users
{
    [Category(new string[] { "Users" })]
    public class UserRoot : Component
    {
        private Vector2 _mousePressedPos;

        private float _moveSpeed = 10.0f;

        private bool _mousePressed = false;

        public SyncRef<Entity> Head;

        public SyncRef<Entity> LeftHand;
        public SyncRef<Entity> RightHand;


        public Matrix4x4 Viewpos => Head.target.globalTrans();

        public override void buildSyncObjs(bool newRefIds)
        {
            Head = new SyncRef<Entity>(this, newRefIds);
            LeftHand = new SyncRef<Entity>(this, newRefIds);
            RightHand = new SyncRef<Entity>(this, newRefIds);
        }

        public override void onLoaded()
        {
            entity.persistence.value = false;
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
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
            lookRotation *= Quaternion.CreateFromYawPitchRoll(Rightvraix.x * -5f * deltaSeconds , 0.0f, 0.0f);

            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.A))
            {
                motionDir += -Vector3.UnitX;
            }
            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.D))
            {
                motionDir += Vector3.UnitX;
            }
            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.W))
            {
                motionDir += -Vector3.UnitZ;
            }
            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.S))
            {
                motionDir += Vector3.UnitZ;
            }
            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.Q))
            {
                motionDir += -Vector3.UnitY;
            }
            if (world.worldManager.engine.inputManager.mainWindows.GetKey(Key.E))
            {
                motionDir += Vector3.UnitY;
            }

            if ((world.worldManager.engine.inputManager.mainWindows.GetMouseButton(MouseButton.Left) || world.worldManager.engine.inputManager.mainWindows.GetMouseButton(MouseButton.Right)))
            {
                if (!_mousePressed)
                {
                    _mousePressed = true;
                    _mousePressedPos = world.worldManager.engine.inputManager.mainWindows.MousePosition;
                    Sdl2Native.SDL_ShowCursor(0);
                    Sdl2Native.SDL_SetWindowGrab(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, true);
                }
                Vector2 mouseDelta = _mousePressedPos - world.worldManager.engine.inputManager.mainWindows.MousePosition;
                Sdl2Native.SDL_WarpMouseInWindow(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                float Yaw = mouseDelta.X * 0.002f;
                float Pitch = mouseDelta.Y * 0.002f;
                lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);

            }
            else if (_mousePressed)
            {
                Sdl2Native.SDL_WarpMouseInWindow(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                Sdl2Native.SDL_SetWindowGrab(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, false);
                Sdl2Native.SDL_ShowCursor(1);
                _mousePressed = false;
            }
            if (motionDir != Vector3.Zero|| lookRotation != default)
            {
                motionDir = Vector3.Transform(motionDir, lookRotation);
                Matrix4x4 addTo = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(lookRotation) * Matrix4x4.CreateTranslation(motionDir * _moveSpeed * sprintFactor * deltaSeconds);
                entity.setGlobalTrans(addTo* entity.globalTrans());
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
