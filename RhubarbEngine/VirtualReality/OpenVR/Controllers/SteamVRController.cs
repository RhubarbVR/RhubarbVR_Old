using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
using RhubarbEngine.Input;
using RhubarbEngine.Input.Controllers;
using OVR = Valve.VR.OpenVR;
using Valve.VR;
using System.Runtime.InteropServices;

namespace RhubarbEngine.VirtualReality.OpenVR.Controllers
{
    internal class SteamVRController : IController, ICosmosController, IKnucklesController, IOculusTouchController, IViveController
    {
        public readonly OpenVRContext openVRContext;

        public readonly string ControllerName;

        public Matrix4x4 MatrixOffset { get; private set; } 

        string IController.ControllerName
        {
            get
            {
                return ControllerName;
            }
        }

        public Creality Creality;
        Creality IController.Creality
        {
            get
            {
                return Creality;
            }
        }

        InputDigitalActionData_t _primaryPressData = new();
        private readonly ulong _generalmPrimaryPressHandle = 0;
        bool IController.PrimaryPress
        {
            get
            {
                return _primaryPressData.bState;
            }
        }

        InputDigitalActionData_t _triggerTouchingData = new();
        private readonly ulong _generalmTriggerTouchingHandle = 0;
        bool IController.TriggerTouching
        {
            get
            {
                return _triggerTouchingData.bState;
            }
        }

        InputDigitalActionData_t _axisTouchingData = new();
		private readonly ulong _generalmAxisTouchingHandle = 0;
        bool IController.AxisTouching
        {
            get
            {
                return _axisTouchingData.bState;
            }
        }

        InputDigitalActionData_t _generalmSystemPressData = new();
		private readonly ulong _generalmSystemPressHandle = 0;
        bool IController.SystemPress
        {
            get
            {
                return _generalmSystemPressData.bState;
            }
        }

        InputDigitalActionData_t _generalmMenuPressData = new();
		private readonly ulong _generalmMenuPressHandle = 0;
        bool IController.MenuPress
        {
            get
            {
                return _generalmMenuPressData.bState;
            }
        }

        InputDigitalActionData_t _generalmGrabPressData = new();
		private readonly ulong _generalmGrabPressHandle = 0;
        bool IController.GrabPress
        {
            get
            {
                return _generalmGrabPressData.bState;
            }
        }

        InputDigitalActionData_t _generalmSecondaryPressData = new();
		private readonly ulong _generalmSecondaryPressHandle = 0;
        bool IController.SecondaryPress
        {
            get
            {
                return _generalmSecondaryPressData.bState;
            }
        }

        InputAnalogActionData_t _generalmAxisData = new();
		private readonly ulong _generalmAxisHandle = 0;
        Vector2f IController.Axis
        {
            get
            {
                return new Vector2f(MakeGoodFloat(_generalmAxisData.x), MakeGoodFloat(_generalmAxisData.y));
            }
        }

        InputAnalogActionData_t _generalmTriggerAixData = new();
		private readonly ulong _generalmTriggerAixHandle = 0;
        float IController.TriggerAix
        {
            get
            {
                return MakeGoodFloat(_generalmTriggerAixData.x);
            }
        }

        InputPoseActionData_t _generalmPosistionData = new();
		private readonly ulong _generalmPosistionHandle = 0;
        Matrix4x4 IController.Posistion
        {
            get
            {
                return PosHelp(_generalmPosistionData.pose.mDeviceToAbsoluteTracking) * MatrixOffset;
            }
        }

        public uint deviceindex;

		public ulong handle;

		public static Quaternion QuaternionFromMatrix(HmdMatrix34_t m)
		{
			var w = Math.Sqrt(1 + m.m0 + m.m5 + m.m10) / 2.0;
			return new Quaternion
			{
				W = (float)w, // Scalar
				X = (float)((m.m9 - m.m6) / (4 * w)),
				Y = (float)((m.m2 - m.m8) / (4 * w)),
				Z = (float)((m.m4 - m.m1) / (4 * w))
			};
		}
		public static float MakeGoodFloat(float val)
		{
			return (float.IsNaN(val) || float.IsInfinity(val)) ? 0f : val;
		}

		public static Matrix4x4 PosHelp(HmdMatrix34_t pos)
		{
			return Matrix4x4.CreateScale(1) * Matrix4x4.CreateFromQuaternion(QuaternionFromMatrix(pos)) * Matrix4x4.CreateTranslation(new Vector3(pos.m3, pos.m7, pos.m11));
		}
		public static string MatToString(HmdMatrix34_t mat)
		{
			return $"[{mat.m0}, {mat.m1}, {mat.m2}, {mat.m3},\n" +
				   $"{mat.m4}, {mat.m5}, {mat.m6}, {mat.m7},\n" +
				   $"{mat.m8}, {mat.m9}, {mat.m10}, {mat.m11}]";
		}


		internal SteamVRController(OpenVRContext openVRContext, string ControllerName, uint deviceindex, Creality Creality, ulong Handle)
		{
			this.openVRContext = openVRContext;
			this.ControllerName = ControllerName;
			Console.WriteLine($"{ControllerName}  index: {deviceindex} Creality:{Creality} Handle: {Handle}");
			this.deviceindex = deviceindex;
			this.Creality = Creality;
			this.handle = Handle;
            MatrixOffset  = ControllerName switch
            {
                "indexController" => Matrix4x4.CreateScale(1f),
                _ => Matrix4x4.CreateScale(1f),
            };


            OVR.Input.GetActionHandle("/actions/General/in/Trigger_Touching", ref _generalmTriggerTouchingHandle);

			OVR.Input.GetActionHandle("/actions/General/in/Axis_Touching", ref _generalmAxisTouchingHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Primary_Pressed", ref _generalmPrimaryPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Secondary_Pressed", ref _generalmSecondaryPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Menu_Pressed", ref _generalmMenuPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/System", ref _generalmSystemPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Grab_Pressed", ref _generalmGrabPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Axis", ref _generalmAxisHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Trigger_Aix", ref _generalmTriggerAixHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Pose", ref _generalmPosistionHandle);

		}

		public void Update()
		{
			var Digsize = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
            var Possize = (uint)Marshal.SizeOf(typeof(InputPoseActionData_t));
            var Analogsize = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));

			var error = OVR.Input.GetDigitalActionData(_generalmTriggerTouchingHandle, ref _triggerTouchingData, Digsize, handle);
			if (error != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errora = OVR.Input.GetDigitalActionData(_generalmAxisTouchingHandle, ref _axisTouchingData, Digsize, handle);
			if (errora != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errorb = OVR.Input.GetDigitalActionData(_generalmPrimaryPressHandle, ref _primaryPressData, Digsize, handle);
			if (errorb != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errorc = OVR.Input.GetDigitalActionData(_generalmSecondaryPressHandle, ref _generalmSecondaryPressData, Digsize, handle);
			if (errorc != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errord = OVR.Input.GetDigitalActionData(_generalmMenuPressHandle, ref _generalmMenuPressData, Digsize, handle);
			if (errord != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errore = OVR.Input.GetDigitalActionData(_generalmSystemPressHandle, ref _generalmSystemPressData, Digsize, handle);
			if (errore != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errori = OVR.Input.GetDigitalActionData(_generalmGrabPressHandle, ref _generalmGrabPressData, Digsize, handle);
			if (errori != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errorf = OVR.Input.GetAnalogActionData(_generalmAxisHandle, ref _generalmAxisData, Analogsize, handle);
			if (errorf != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errorg = OVR.Input.GetAnalogActionData(_generalmTriggerAixHandle, ref _generalmTriggerAixData, Analogsize, handle);
			if (errorg != 0)
			{
                Console.WriteLine(error.ToString());
			}
			var errorh = OVR.Input.GetPoseActionDataRelativeToNow(_generalmPosistionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, ref _generalmPosistionData, Possize, handle);
			if (errorh != 0)
			{
                Console.WriteLine(error.ToString());
			}
		}


        Vector2f ICosmosController.Joystick_Aixs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Joystick_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Joystick_Clicked
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float ICosmosController.Trigger_Aix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Trigger_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Trigger_Clicked
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Grip_Clicked
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Menu_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.AX_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.BY_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICosmosController.Bumper
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Grip_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.A_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.B_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.A_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.B_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IKnucklesController.Trigger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Trigger_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Trigger_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Joystick_Clicked
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Vector2f IKnucklesController.Touchpad_Aixs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Touchpad_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Youchpad_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Vector2f IKnucklesController.Joystick_Axis
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Joystick_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IKnucklesController.Grip_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IKnucklesController.Touchpad_Force
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.YB_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.XA_Pressed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.YB_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.XA_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Vector2f IOculusTouchController.Joystick_Aixs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Joystick_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Joystick_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Thumb_Rest_Touched
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IOculusTouchController.Grip_Aix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Grip_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IOculusTouchController.Trigger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Trigger_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IOculusTouchController.Trigger_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.Touchpad_Touch
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.Touchpad_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Vector2f IViveController.Touchpad_Aixs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.Trigger_Hair
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.Trigger_Click
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IViveController.Trigger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.Grip
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.System
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IViveController.App
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
