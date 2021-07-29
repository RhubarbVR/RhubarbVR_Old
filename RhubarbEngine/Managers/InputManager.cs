using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Input;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Input.Controllers;
using g3;
using System.Numerics;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Managers
{
    public class InputManager : IManager
    {
        private Engine engine;

        private KeyboardStealer _keyboard;

        public KeyboardStealer keyboard { get { return _keyboard; } set { if (value != null) { engine.worldManager.personalSpace?.OpenKeyboard(); } else { engine.worldManager.personalSpace?.CloseKeyboard(); } _keyboard = value; } }

        public bool isKeyboardinuse => keyboard != null;

        public InputTracker mainWindows = new InputTracker();

        public event Action removeFocus;

        public void RemoveFocus()
        {
            removeFocus?.Invoke();
        }

        public IController leftController => engine.renderManager.vrContext.leftController;

        public IController RightController => engine.renderManager.vrContext.RightController;

        public string ControllerName(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return (leftController != null) ? leftController.ControllerName : "null";
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.ControllerName : "null";
                    break;
                default:
                    string r = (RightController != null) ? RightController.ControllerName : "null";
                    string l = (leftController != null) ? leftController.ControllerName : "null";
                    return $"R{r}  L{l}";
                    break;
            }

        }
        public bool PrimaryPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.PrimaryPress : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.PrimaryPress : false;
                    break;
                default:
                    return (RightController != null) ? RightController.PrimaryPress : false || (leftController != null) ? leftController.PrimaryPress : false;
                    break;
            }
        }

        public bool TriggerTouching(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.TriggerTouching : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.TriggerTouching : false;
                    break;
                default:
                    return (RightController != null) ? RightController.TriggerTouching : false || (leftController != null) ? leftController.TriggerTouching : false;
                    break;
            }
        }

        public bool AxisTouching(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.AxisTouching : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.AxisTouching : false;
                    break;
                default:
                    return (RightController != null) ? RightController.AxisTouching : false || (leftController != null) ? leftController.AxisTouching : false;
                    break;
            }
        }

        public bool SystemPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.SystemPress : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.SystemPress : false;
                    break;
                default:
                    return (RightController != null) ? RightController.SystemPress : false || (leftController != null) ? leftController.SystemPress : false;
                    break;
            }
        }

        public bool MenuPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.MenuPress : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.MenuPress : false;
                    break;
                default:
                    return (RightController != null) ? RightController.MenuPress : false || (leftController != null) ? leftController.MenuPress : false;
                    break;
            }
        }

        public bool GrabPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:

                    return (leftController != null) ? leftController.GrabPress : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.GrabPress : false;
                    break;
                default:
                    return (RightController != null) ? RightController.GrabPress : false || (leftController != null) ? leftController.GrabPress : false;
                    break;
            }
        }

        public bool SecondaryPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    
                    return (leftController != null)? leftController.SecondaryPress : false;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.SecondaryPress : false;
                    break;
                default:
                    return (RightController != null) ? RightController.SecondaryPress : false || (leftController != null) ? leftController.SecondaryPress : false;
                    break;
            }
        }

        public Vector2f Axis(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return (leftController != null) ? leftController.Axis : Vector2f.Zero;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.Axis : Vector2f.Zero;
                    break;
                default:
                    return (((RightController != null) ? RightController.Axis : Vector2f.Zero ) +( (leftController != null) ? leftController.Axis : Vector2f.Zero)) / 2;
                    break;
            }
        }

        public Matrix4x4 GetPos(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return (leftController != null) ? leftController.Posistion : Matrix4x4.CreateScale(1f);
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.Posistion : Matrix4x4.CreateScale(1f);
                    break;
                default:
                    return Matrix4x4.CreateScale(1f);
                    break;
            }
        }

        public float TriggerAix(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return (leftController != null) ? leftController.TriggerAix : 0f;
                    break;
                case Creality.Right:
                    return (RightController != null) ? RightController.TriggerAix : 0f;
                    break;
                default:
                    return (((RightController != null) ? RightController.TriggerAix : 0f) + ((leftController != null) ? leftController.TriggerAix : 0f)) / 2;
                    break;
            }
        }

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            return this;
        }

        public void Update()
        {
            if (mainWindows.GetKeyDown(Key.F8))
            {
                if (engine.outputType == OutputType.Screen)
                {
                    engine.renderManager.switchVRContext(OutputType.SteamVR);
                }
                else
                {
                    if (engine.outputType == OutputType.SteamVR)
                    {
                        engine.renderManager.switchVRContext(OutputType.Screen);
                    }
                }
            }
        }
    }
}
