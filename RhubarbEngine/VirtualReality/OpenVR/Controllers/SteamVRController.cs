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
		string IController.ControllerName => ControllerName;

		public Creality Creality;
		Creality IController.Creality => Creality;

		InputDigitalActionData_t PrimaryPressData = new InputDigitalActionData_t();
		ulong GeneralmPrimaryPressHandle = 0;
		bool IController.PrimaryPress => PrimaryPressData.bState;

		InputDigitalActionData_t TriggerTouchingData = new InputDigitalActionData_t();
		private ulong GeneralmTriggerTouchingHandle = 0;
		bool IController.TriggerTouching => TriggerTouchingData.bState;

		InputDigitalActionData_t AxisTouchingData = new InputDigitalActionData_t();
		private ulong GeneralmAxisTouchingHandle = 0;
		bool IController.AxisTouching => AxisTouchingData.bState;

		InputDigitalActionData_t GeneralmSystemPressData = new InputDigitalActionData_t();
		private ulong GeneralmSystemPressHandle = 0;
		bool IController.SystemPress => GeneralmSystemPressData.bState;

		InputDigitalActionData_t GeneralmMenuPressData = new InputDigitalActionData_t();
		private ulong GeneralmMenuPressHandle = 0;
		bool IController.MenuPress => GeneralmMenuPressData.bState;

		InputDigitalActionData_t GeneralmGrabPressData = new InputDigitalActionData_t();
		private ulong GeneralmGrabPressHandle = 0;
		bool IController.GrabPress => GeneralmGrabPressData.bState;

		InputDigitalActionData_t GeneralmSecondaryPressData = new InputDigitalActionData_t();
		private ulong GeneralmSecondaryPressHandle = 0;
		bool IController.SecondaryPress => GeneralmSecondaryPressData.bState;

		InputAnalogActionData_t GeneralmAxisData = new InputAnalogActionData_t();
		private ulong GeneralmAxisHandle = 0;
		Vector2f IController.Axis => new Vector2f(MakeGoodFloat(GeneralmAxisData.x), MakeGoodFloat(GeneralmAxisData.y));


		InputAnalogActionData_t GeneralmTriggerAixData = new InputAnalogActionData_t();
		private ulong GeneralmTriggerAixHandle = 0;
		float IController.TriggerAix => MakeGoodFloat(GeneralmTriggerAixData.x);

		InputPoseActionData_t GeneralmPosistionData = new InputPoseActionData_t();
		private ulong GeneralmPosistionHandle = 0;
		Matrix4x4 IController.Posistion => posHelp(GeneralmPosistionData.pose.mDeviceToAbsoluteTracking);

		public uint deviceindex;

		public ulong handle;
		private static Matrix4x4 ToSysMatrix(HmdMatrix34_t hmdMat)
		{
			return new Matrix4x4(
				hmdMat.m0, hmdMat.m4, hmdMat.m8, 0f,
				hmdMat.m1, hmdMat.m5, hmdMat.m9, 0f,
				hmdMat.m2, hmdMat.m6, hmdMat.m10, 0f,
				hmdMat.m3, hmdMat.m7, hmdMat.m11, 1f);
		}


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
			return ((val == float.NaN) || (float.IsInfinity(val))) ? 0f : val;
		}

		public static Matrix4x4 posHelp(HmdMatrix34_t pos)
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
			Logger.Log($"{ControllerName}  index: {deviceindex} Creality:{Creality} Handle: {Handle}");
			this.deviceindex = deviceindex;
			this.Creality = Creality;
			this.handle = Handle;

			OVR.Input.GetActionHandle("/actions/General/in/Trigger_Touching", ref GeneralmTriggerTouchingHandle);

			OVR.Input.GetActionHandle("/actions/General/in/Axis_Touching", ref GeneralmAxisTouchingHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Primary_Pressed", ref GeneralmPrimaryPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Secondary_Pressed", ref GeneralmSecondaryPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Menu_Pressed", ref GeneralmMenuPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/System", ref GeneralmSystemPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Grab_Pressed", ref GeneralmGrabPressHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Axis", ref GeneralmAxisHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Trigger_Aix", ref GeneralmTriggerAixHandle);
			OVR.Input.GetActionHandle("/actions/General/in/Pose", ref GeneralmPosistionHandle);

		}

		public void update()
		{
			uint Digsize = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
			uint Possize = (uint)Marshal.SizeOf(typeof(InputPoseActionData_t));
			uint Analogsize = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));

			var error = OVR.Input.GetDigitalActionData(GeneralmTriggerTouchingHandle, ref TriggerTouchingData, Digsize, handle);
			if (error != 0)
			{
				Logger.Log(error.ToString());
			}
			var errora = OVR.Input.GetDigitalActionData(GeneralmAxisTouchingHandle, ref AxisTouchingData, Digsize, handle);
			if (errora != 0)
			{
				Logger.Log(error.ToString());
			}
			var errorb = OVR.Input.GetDigitalActionData(GeneralmPrimaryPressHandle, ref PrimaryPressData, Digsize, handle);
			if (errorb != 0)
			{
				Logger.Log(error.ToString());
			}
			var errorc = OVR.Input.GetDigitalActionData(GeneralmSecondaryPressHandle, ref GeneralmSecondaryPressData, Digsize, handle);
			if (errorc != 0)
			{
				Logger.Log(error.ToString());
			}
			var errord = OVR.Input.GetDigitalActionData(GeneralmMenuPressHandle, ref GeneralmMenuPressData, Digsize, handle);
			if (errord != 0)
			{
				Logger.Log(error.ToString());
			}
			var errore = OVR.Input.GetDigitalActionData(GeneralmSystemPressHandle, ref GeneralmSystemPressData, Digsize, handle);
			if (error != 0)
			{
				Logger.Log(error.ToString());
			}
			var errori = OVR.Input.GetDigitalActionData(GeneralmGrabPressHandle, ref GeneralmGrabPressData, Digsize, handle);
			if (errori != 0)
			{
				Logger.Log(error.ToString());
			}
			var errorf = OVR.Input.GetAnalogActionData(GeneralmAxisHandle, ref GeneralmAxisData, Analogsize, handle);
			if (errorf != 0)
			{
				Logger.Log(error.ToString());
			}
			var errorg = OVR.Input.GetAnalogActionData(GeneralmTriggerAixHandle, ref GeneralmTriggerAixData, Analogsize, handle);
			if (errorg != 0)
			{
				Logger.Log(error.ToString());
			}
			var errorh = OVR.Input.GetPoseActionDataRelativeToNow(GeneralmPosistionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, ref GeneralmPosistionData, Possize, handle);
			if (errorh != 0)
			{
				Logger.Log(error.ToString());
			}
		}


		Vector2f ICosmosController.Joystick_Aixs => throw new NotImplementedException();

		bool ICosmosController.Joystick_Touched => throw new NotImplementedException();

		bool ICosmosController.Joystick_Clicked => throw new NotImplementedException();

		float ICosmosController.Trigger_Aix => throw new NotImplementedException();

		bool ICosmosController.Trigger_Touched => throw new NotImplementedException();

		bool ICosmosController.Trigger_Clicked => throw new NotImplementedException();

		bool ICosmosController.Grip_Clicked => throw new NotImplementedException();

		bool ICosmosController.Menu_Pressed => throw new NotImplementedException();

		bool ICosmosController.AX_Pressed => throw new NotImplementedException();

		bool ICosmosController.BY_Pressed => throw new NotImplementedException();

		bool ICosmosController.Bumper => throw new NotImplementedException();

		bool IKnucklesController.Grip_Touch => throw new NotImplementedException();

		bool IKnucklesController.A_Pressed => throw new NotImplementedException();

		bool IKnucklesController.B_Pressed => throw new NotImplementedException();

		bool IKnucklesController.A_Touch => throw new NotImplementedException();

		bool IKnucklesController.B_Touch => throw new NotImplementedException();

		float IKnucklesController.Trigger => throw new NotImplementedException();

		bool IKnucklesController.Trigger_Touched => throw new NotImplementedException();

		bool IKnucklesController.Trigger_Click => throw new NotImplementedException();

		bool IKnucklesController.Joystick_Clicked => throw new NotImplementedException();

		Vector2f IKnucklesController.Touchpad_Aixs => throw new NotImplementedException();

		bool IKnucklesController.Touchpad_Touched => throw new NotImplementedException();

		bool IKnucklesController.Youchpad_Pressed => throw new NotImplementedException();

		Vector2f IKnucklesController.Joystick_Axis => throw new NotImplementedException();

		bool IKnucklesController.Joystick_Touched => throw new NotImplementedException();

		bool IKnucklesController.Grip_Click => throw new NotImplementedException();

		float IKnucklesController.Touchpad_Force => throw new NotImplementedException();

		bool IOculusTouchController.YB_Pressed => throw new NotImplementedException();

		bool IOculusTouchController.XA_Pressed => throw new NotImplementedException();

		bool IOculusTouchController.YB_Touched => throw new NotImplementedException();

		bool IOculusTouchController.XA_Touched => throw new NotImplementedException();

		Vector2f IOculusTouchController.Joystick_Aixs => throw new NotImplementedException();

		bool IOculusTouchController.Joystick_Touch => throw new NotImplementedException();

		bool IOculusTouchController.Joystick_Click => throw new NotImplementedException();

		bool IOculusTouchController.Thumb_Rest_Touched => throw new NotImplementedException();

		float IOculusTouchController.Grip_Aix => throw new NotImplementedException();

		bool IOculusTouchController.Grip_Click => throw new NotImplementedException();

		float IOculusTouchController.Trigger => throw new NotImplementedException();

		bool IOculusTouchController.Trigger_Touch => throw new NotImplementedException();

		bool IOculusTouchController.Trigger_Click => throw new NotImplementedException();

		bool IViveController.Touchpad_Touch => throw new NotImplementedException();

		bool IViveController.Touchpad_Click => throw new NotImplementedException();

		Vector2f IViveController.Touchpad_Aixs => throw new NotImplementedException();

		bool IViveController.Trigger_Hair => throw new NotImplementedException();

		bool IViveController.Trigger_Click => throw new NotImplementedException();

		float IViveController.Trigger => throw new NotImplementedException();

		bool IViveController.Grip => throw new NotImplementedException();

		bool IViveController.System => throw new NotImplementedException();

		bool IViveController.App => throw new NotImplementedException();

	}
}
