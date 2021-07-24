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
using BulletSharp;
using BulletSharp.Math;
using Veldrid;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.Physics.Colliders
{

    [Category(new string[] { "Physics/Colliders" })]
    public class InputPlane : Collider, IinputPlane
    {
        public Sync<Vector2f> size;

        public Sync<Vector2u> pixelSize;

        public Sync<float> depth;

        public Sync<bool> FocusedOverride;

        public IReadOnlyList<KeyEvent> KeyEvents { get { if (!focused) return new List<KeyEvent>(); return input.mainWindows.FrameSnapshot.KeyEvents; } }

        public IReadOnlyList<MouseEvent> MouseEvents { get { if (!focused) return new List<MouseEvent>(); return input.mainWindows.FrameSnapshot.MouseEvents; } }

        public IReadOnlyList<char> KeyCharPresses { get { if (!focused) return new List<Char>(); return input.mainWindows.FrameSnapshot.KeyCharPresses;  } }

        private Vector2 mousePosition = Vector2.Zero;

        public Vector2 MousePosition => mousePosition;


        public void Click(Vector2 pos, InteractionSource sourc)
        {
            Setfocused();
            val = sourc;
            mousePosition = pos;
            StopMousePos = false;
        }

        public void updatePos(Vector2 pos, InteractionSource sourc)
        {
            if (StopMousePos) return;
            if (sourc != val) return;
            mousePosition = pos;
        }

        public float WheelDelta
        {
            get
            {
                if (!focused) return 0f;
                return input.mainWindows.FrameSnapshot.WheelDelta;
            }
        }

        private InteractionSource val = InteractionSource.None;

        public InteractionSource source => val;

        private bool _focused = false;

        public bool focused => _focused;

        public bool StopMousePos = false;

        public bool StopMouse { get { return StopMousePos; } set { StopMousePos = value; } }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            size = new Sync<Vector2f>(this, newRefIds);
            size.value = new Vector2f(0.5f);
            depth = new Sync<float>(this, newRefIds);
            depth.value = 0.01f;
            pixelSize = new Sync<Vector2u>(this, newRefIds);
            pixelSize.value = new Vector2u(600, 600);
            FocusedOverride = new Sync<bool>(this, newRefIds);
            size.Changed += UpdateChange;
        }

        public void UpdateChange(IChangeable val)
        {
            BuildShape();
        }

        public override void onLoaded()
        {
            base.onLoaded();
            BuildShape();
        }

        public override void BuildShape()
        {
            startShape(new BoxShape(new BulletSharp.Math.Vector3(size.value.x, depth.value, size.value.y)));
        }

        public bool IsMouseDown(MouseButton button)
        {
            if (StopMousePos) return false;
            if (!focused) return false;
            switch (source)
            {
                case InteractionSource.None:
                    break;
                case InteractionSource.LeftLaser:
                    switch (button)
                    {
                        case MouseButton.Left:
                            return input.PrimaryPress(Input.Creality.Left);
                            break;
                        case MouseButton.Middle:
                            return input.TriggerTouching(Input.Creality.Left);
                            break;
                        case MouseButton.Right:
                            return input.SecondaryPress(Input.Creality.Left);
                            break;
                        case MouseButton.Button1:
                            break;
                        case MouseButton.Button2:
                            break;
                        case MouseButton.Button3:
                            break;
                        case MouseButton.Button4:
                            break;
                        case MouseButton.Button5:
                            break;
                        case MouseButton.Button6:
                            break;
                        case MouseButton.Button7:
                            break;
                        case MouseButton.Button8:
                            break;
                        case MouseButton.Button9:
                            break;
                        case MouseButton.LastButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case InteractionSource.LeftFinger:
                    break;
                case InteractionSource.RightLaser:
                    switch (button)
                    {
                        case MouseButton.Left:
                            return input.PrimaryPress(Input.Creality.Right);
                            break;
                        case MouseButton.Middle:
                            return input.TriggerTouching(Input.Creality.Right);
                            break;
                        case MouseButton.Right:
                            return input.SecondaryPress(Input.Creality.Right);
                            break;
                        case MouseButton.Button1:
                            break;
                        case MouseButton.Button2:
                            break;
                        case MouseButton.Button3:
                            break;
                        case MouseButton.Button4:
                            break;
                        case MouseButton.Button5:
                            break;
                        case MouseButton.Button6:
                            break;
                        case MouseButton.Button7:
                            break;
                        case MouseButton.Button8:
                            break;
                        case MouseButton.Button9:
                            break;
                        case MouseButton.LastButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case InteractionSource.RightFinger:
                    break;
                case InteractionSource.HeadLaser:
                    return engine.inputManager.mainWindows.GetMouseButtonDown(button);
                    break;
                case InteractionSource.HeadFinger:
                    break;
                default:
                    break;
            }
            return false;
        }

        public void Setfocused()
        {
            if (_focused) {
                return;
            }
            _focused = true;
            if (!FocusedOverride.value)
            {
                input.RemoveFocus();
            }
            input.removeFocus += Removefocused;
        }

        public void Removefocused()
        {
            input.removeFocus -= Removefocused;
            _focused = false;
        }

        public InputPlane(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public InputPlane()
        {
        }
    }
}
