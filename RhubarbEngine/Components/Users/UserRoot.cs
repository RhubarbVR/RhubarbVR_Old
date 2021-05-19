using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using BaseR;
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
        private Vector3 _position = new Vector3(0, 3, 0);

        private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);

        private Vector2 _mousePressedPos;

        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

        private float _moveSpeed = 10.0f;

        private float _yaw;

        private float _pitch;

        private bool _mousePressed = false;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public override void buildSyncObjs(bool newRefIds)
        {

        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            float deltaSeconds = 0.01f;
            float sprintFactor = world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ControlLeft)
               ? 0.1f
               : world.worldManager.engine.inputManager.mainWindows.GetKey(Key.ShiftLeft)
               ? 2.5f
                 : 1f;
            Vector3 motionDir = Vector3.Zero;
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
                Yaw += mouseDelta.X * 0.002f;
                Pitch += mouseDelta.Y * 0.002f;
            }
            else if (_mousePressed)
            {
                Sdl2Native.SDL_WarpMouseInWindow(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                Sdl2Native.SDL_SetWindowGrab(world.worldManager.engine.windowManager.mainWindow.window.SdlWindowHandle, false);
                Sdl2Native.SDL_ShowCursor(1);
                _mousePressed = false;
            }
            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
                motionDir = Vector3.Transform(motionDir, lookRotation);
                _position += motionDir * _moveSpeed * sprintFactor * deltaSeconds;
                UpdateViewMatrix();
            }
        }
        private void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            _lookDirection = lookDir;
            entity.setGlobalTrans(Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY));
        }
        public UserRoot(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public UserRoot()
        {
        }
    }
}
