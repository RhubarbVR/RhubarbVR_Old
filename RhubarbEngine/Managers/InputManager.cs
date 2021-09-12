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
		private Engine _engine;

		private IKeyboardStealer _keyboard;

		public IKeyboardStealer Keyboard { get { return _keyboard; } set { if (value != null) { _engine.worldManager.personalSpace?.OpenKeyboard(); } else { _engine.worldManager.personalSpace?.CloseKeyboard(); } _keyboard = value; } }

        public bool IsKeyboardinuse
        {
            get
            {
                return Keyboard != null;
            }
        }

        public InputTracker mainWindows = new InputTracker();

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
                return _engine.renderManager.vrContext.leftController;
            }
        }

        public IController RightController
        {
            get
            {
                return _engine.renderManager.vrContext.RightController;
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
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.PrimaryPress;
				case Creality.Right:
					return (RightController != null) && RightController.PrimaryPress;
				default:
					return (RightController != null) ? RightController.PrimaryPress : (false || (LeftController != null)) && LeftController.PrimaryPress;
			}
		}

		public bool TriggerTouching(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.TriggerTouching;
				case Creality.Right:
					return (RightController != null) && RightController.TriggerTouching;
				default:
					return (RightController != null) ? RightController.TriggerTouching : (false || (LeftController != null)) && LeftController.TriggerTouching;
			}
		}

		public bool AxisTouching(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.AxisTouching;
				case Creality.Right:
					return (RightController != null) && RightController.AxisTouching;
				default:
					return (RightController != null) ? RightController.AxisTouching : (false || (LeftController != null)) && LeftController.AxisTouching;
			}
		}

		public bool SystemPress(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.SystemPress;
				case Creality.Right:
					return (RightController != null) && RightController.SystemPress;
				default:
					return (RightController != null) ? RightController.SystemPress : (false || (LeftController != null)) && LeftController.SystemPress;
			}
		}

		public bool MenuPress(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.MenuPress;
				case Creality.Right:
					return (RightController != null) && RightController.MenuPress;
				default:
					return (RightController != null) ? RightController.MenuPress : (false || (LeftController != null)) && LeftController.MenuPress;
			}
		}

		public bool GrabPress(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:

					return (LeftController != null) && LeftController.GrabPress;
				case Creality.Right:
					return (RightController != null) && RightController.GrabPress;
				default:
					return (RightController != null) ? RightController.GrabPress : (false || (LeftController != null)) && LeftController.GrabPress;
			}
		}

		public bool SecondaryPress(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) && LeftController.SecondaryPress;
				case Creality.Right:
					return (RightController != null) && RightController.SecondaryPress;
				default:
					return (RightController != null) ? RightController.SecondaryPress : (false || (LeftController != null)) && LeftController.SecondaryPress;
			}
		}

		public Vector2f Axis(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) ? LeftController.Axis : Vector2f.Zero;
				case Creality.Right:
					return (RightController != null) ? RightController.Axis : Vector2f.Zero;
				default:
					return (((RightController != null) ? RightController.Axis : Vector2f.Zero) + ((LeftController != null) ? LeftController.Axis : Vector2f.Zero)) / 2;
			}
		}

		public Matrix4x4 GetPos(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) ? LeftController.Posistion : Matrix4x4.CreateScale(1f);
				case Creality.Right:
					return (RightController != null) ? RightController.Posistion : Matrix4x4.CreateScale(1f);
				default:
					return Matrix4x4.CreateScale(1f);
			}
		}

		public float TriggerAix(Creality side = Creality.None)
		{
			switch (side)
			{
				case Creality.Left:
					return (LeftController != null) ? LeftController.TriggerAix : 0f;
				case Creality.Right:
					return (RightController != null) ? RightController.TriggerAix : 0f;
				default:
					return (((RightController != null) ? RightController.TriggerAix : 0f) + ((LeftController != null) ? LeftController.TriggerAix : 0f)) / 2;
			}
		}

		public IManager initialize(Engine _engine)
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
				if (_engine.outputType == OutputType.Screen)
				{
					_engine.renderManager.SwitchVRContext(OutputType.SteamVR);
				}
				else
				{
					if (_engine.outputType == OutputType.SteamVR)
					{
						_engine.renderManager.SwitchVRContext(OutputType.Screen);
					}
				}
			}
		}
	}
}
