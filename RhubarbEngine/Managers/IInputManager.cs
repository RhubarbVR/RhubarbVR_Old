using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Input;
using Veldrid;
using RhubarbEngine.VirtualReality;
using RhubarbEngine.Input.Controllers;
using RNumerics;
using System.Numerics;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Managers
{
    public interface IInputManager: IManager
    {
        public IKeyboardStealer Keyboard { get; set; }
        public InputTracker MainWindows { get; }
        public bool IsKeyboardinuse { get; }
        public InteractionLaserSource LeftLaser { get; }

        public InteractionLaserSource RightLaser { get; }

        event Action RemoveFocus;

        Vector2f Axis(Creality side = Creality.None);
        bool AxisTouching(Creality side = Creality.None);
        string ControllerName(Creality side = Creality.None);
        Matrix4x4 GetPos(Creality side = Creality.None);
        bool GrabPress(Creality side = Creality.None);
        void InvokeRemoveFocus();
        bool MenuPress(Creality side = Creality.None);
        bool PrimaryPress(Creality side = Creality.None);
        bool SecondaryPress(Creality side = Creality.None);
        bool SystemPress(Creality side = Creality.None);
        float TriggerAix(Creality side = Creality.None);
        bool TriggerTouching(Creality side = Creality.None);
    }

    public class InputManager : IInputManager
    {
		private IEngine _engine;

		private IKeyboardStealer _keyboard;

		public IKeyboardStealer Keyboard { get { return _keyboard; } set { if (value != null) { _engine.WorldManager.personalSpace?.OpenKeyboard(); } else { _engine.WorldManager.personalSpace?.CloseKeyboard(); } _keyboard = value; } }

        public bool IsKeyboardinuse
        {
            get
            {
                return Keyboard != null;
            }
        }

        public InputTracker mainWindows = new();

		public event Action RemoveFocus;

		public InteractionLaserSource LeftLaser;

		public InteractionLaserSource RightLaser;

		public void InvokeRemoveFocus()
		{
			RemoveFocus?.Invoke();
		}

        public IController LeftController
        {
            get
            {
                return _engine.RenderManager.vrContext.LeftController;
            }
        }

        public IController RightController
        {
            get
            {
                return _engine.RenderManager.vrContext.RightController;
            }
        }

        public InputTracker MainWindows
        {
            get
            {
                return mainWindows;
            }
        }

        InteractionLaserSource IInputManager.LeftLaser
        {
            get
            {
                return LeftLaser;
            }
        }

        InteractionLaserSource IInputManager.RightLaser
        {
            get
            {
                return RightLaser;
            }
        }

        public string ControllerName(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) ? LeftController.ControllerName : "null";
				case Creality.Right:
					return (RightController != null) ? RightController.ControllerName : "null";
				default:
					var r = (RightController != null) ? RightController.ControllerName : "null";
					var l = (LeftController != null) ? LeftController.ControllerName : "null";
					return $"R{r}  L{l}";
			}

		}
		public bool PrimaryPress(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.PrimaryPress,
                Creality.Right => (RightController != null) && RightController.PrimaryPress,
                _ => (RightController != null) ? RightController.PrimaryPress : (false || (LeftController != null)) && LeftController.PrimaryPress,
            };
        }

		public bool TriggerTouching(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.TriggerTouching,
                Creality.Right => (RightController != null) && RightController.TriggerTouching,
                _ => (RightController != null) ? RightController.TriggerTouching : (false || (LeftController != null)) && LeftController.TriggerTouching,
            };
        }

		public bool AxisTouching(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.AxisTouching,
                Creality.Right => (RightController != null) && RightController.AxisTouching,
                _ => (RightController != null) ? RightController.AxisTouching : (false || (LeftController != null)) && LeftController.AxisTouching,
            };
        }

		public bool SystemPress(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.SystemPress,
                Creality.Right => (RightController != null) && RightController.SystemPress,
                _ => (RightController != null) ? RightController.SystemPress : (false || (LeftController != null)) && LeftController.SystemPress,
            };
        }

		public bool MenuPress(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.MenuPress,
                Creality.Right => (RightController != null) && RightController.MenuPress,
                _ => (RightController != null) ? RightController.MenuPress : (false || (LeftController != null)) && LeftController.MenuPress,
            };
        }

		public bool GrabPress(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.GrabPress,
                Creality.Right => (RightController != null) && RightController.GrabPress,
                _ => (RightController != null) ? RightController.GrabPress : (false || (LeftController != null)) && LeftController.GrabPress,
            };
        }

		public bool SecondaryPress(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) && LeftController.SecondaryPress,
                Creality.Right => (RightController != null) && RightController.SecondaryPress,
                _ => (RightController != null) ? RightController.SecondaryPress : (false || (LeftController != null)) && LeftController.SecondaryPress,
            };
        }

		public Vector2f Axis(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) ? LeftController.Axis : Vector2f.Zero,
                Creality.Right => (RightController != null) ? RightController.Axis : Vector2f.Zero,
                _ => (((RightController != null) ? RightController.Axis : Vector2f.Zero) + ((LeftController != null) ? LeftController.Axis : Vector2f.Zero)) / 2,
            };
        }

		public Matrix4x4 GetPos(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) ? LeftController.Posistion : Matrix4x4.CreateScale(1f),
                Creality.Right => (RightController != null) ? RightController.Posistion : Matrix4x4.CreateScale(1f),
                _ => Matrix4x4.CreateScale(1f),
            };
        }

		public float TriggerAix(Creality side = Creality.None)
		{
            return side switch
            {
                Creality.Left => (LeftController != null) ? LeftController.TriggerAix : 0f,
                Creality.Right => (RightController != null) ? RightController.TriggerAix : 0f,
                _ => (((RightController != null) ? RightController.TriggerAix : 0f) + ((LeftController != null) ? LeftController.TriggerAix : 0f)) / 2,
            };
        }

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;

            LeftLaser = new InteractionLaserSource(Creality.Left, this._engine);
            RightLaser = new InteractionLaserSource(Creality.Right, this._engine);

			return this;
		}

		public void Update()
		{
			LeftLaser.Update();
			RightLaser.Update();
			if (mainWindows.GetKeyDown(Key.F8))
			{
				if (_engine.OutputType == OutputType.Screen)
				{
					_engine.RenderManager.SwitchVRContext(OutputType.SteamVR);
				}
				else
				{
					if (_engine.OutputType == OutputType.SteamVR)
					{
						_engine.RenderManager.SwitchVRContext(OutputType.Screen);
					}
				}
			}
		}
	}
}
