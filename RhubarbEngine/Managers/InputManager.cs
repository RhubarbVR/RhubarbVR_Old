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

namespace RhubarbEngine.Managers
{
    public class InputManager : IManager
    {
        private Engine engine;

        public InputTracker mainWindows = new InputTracker();

        public IController leftController => engine.renderManager.vrContext.leftController;

        public IController RightController => engine.renderManager.vrContext.RightController;

        public string ControllerName(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.ControllerName;
                    break;
                case Creality.Right:
                    return RightController.ControllerName;
                    break;
                default:
                    return $"R{RightController.ControllerName}  L{leftController.ControllerName}";
                    break;
            }
        }
        public bool PrimaryPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.PrimaryPress;
                    break;
                case Creality.Right:
                    return RightController.PrimaryPress;
                    break;
                default:
                    return RightController.PrimaryPress || leftController.PrimaryPress;
                    break;
            }
        }

        public bool TriggerTouching(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.TriggerTouching;
                    break;
                case Creality.Right:
                    return RightController.TriggerTouching;
                    break;
                default:
                    return RightController.TriggerTouching || leftController.TriggerTouching;
                    break;
            }
        }

        public bool AxisTouching(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.AxisTouching;
                    break;
                case Creality.Right:
                    return RightController.AxisTouching;
                    break;
                default:
                    return RightController.AxisTouching || leftController.AxisTouching;
                    break;
            }
        }

        public bool SystemPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.SystemPress;
                    break;
                case Creality.Right:
                    return RightController.SystemPress;
                    break;
                default:
                    return RightController.SystemPress || leftController.SystemPress;
                    break;
            }
        }

        public bool MenuPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.MenuPress;
                    break;
                case Creality.Right:
                    return RightController.MenuPress;
                    break;
                default:
                    return RightController.MenuPress || leftController.MenuPress;
                    break;
            }
        }

        public bool GrabPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.GrabPress;
                    break;
                case Creality.Right:
                    return RightController.GrabPress;
                    break;
                default:
                    return RightController.GrabPress || leftController.GrabPress;
                    break;
            }
        }

        public bool SecondaryPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.SecondaryPress;
                    break;
                case Creality.Right:
                    return RightController.SecondaryPress;
                    break;
                default:
                    return RightController.SecondaryPress || leftController.SecondaryPress;
                    break;
            }
        }

        public bool TriggerPress(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.TriggerPress;
                    break;
                case Creality.Right:
                    return RightController.TriggerPress;
                    break;
                default:
                    return RightController.TriggerPress || leftController.TriggerPress;
                    break;
            }
        }

        public Vector2f Axis(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.Axis;
                    break;
                case Creality.Right:
                    return RightController.Axis;
                    break;
                default:
                    return (RightController.Axis + leftController.Axis) / 2;
                    break;
            }
        }

        public float TriggerAix(Creality side = Creality.None)
        {
            switch (side)
            {
                case Creality.Left:
                    return leftController.TriggerAix;
                    break;
                case Creality.Right:
                    return RightController.TriggerAix;
                    break;
                default:
                    return (RightController.TriggerAix + leftController.TriggerAix) / 2;
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
