using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using g3;
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

        internal InputDigitalActionData_t PrimaryPressData = new InputDigitalActionData_t();
        internal ulong GeneralmPrimaryPressHandle = 0;
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
        Vector2f IController.Axis => new Vector2f(GeneralmAxisData.x, GeneralmAxisData.y);


        InputAnalogActionData_t GeneralmTriggerAixData = new InputAnalogActionData_t();
        private ulong GeneralmTriggerAixHandle = 0;
        float IController.TriggerAix => GeneralmTriggerAixData.x;

        InputPoseActionData_t GeneralmPosistionData = new InputPoseActionData_t();
        private ulong GeneralmPosistionHandle = 0;
        Matrix4x4 IController.Posistion => posHelp(GeneralmPosistionData.pose.mDeviceToAbsoluteTracking);
        
        public uint deviceindex;

        public ulong handle;

        public static Matrix4x4 posHelp(HmdMatrix34_t pos)
        {
            Quaternion q = new Quaternion();
            q.W = (float)Math.Sqrt(Math.Max(0, 1 + pos.m0 + pos.m5 + pos.m10)) / 2;
            q.X = (float)Math.Sqrt(Math.Max(0, 1 + pos.m0 - pos.m5 - pos.m10)) / 2;
            q.Y = (float)Math.Sqrt(Math.Max(0, 1 - pos.m0 + pos.m5 - pos.m10)) / 2;
            q.Z = (float)Math.Sqrt(Math.Max(0, 1 - pos.m0 - pos.m5 + pos.m10)) / 2;
            q.X = (float)Math.CopySign(q.X, pos.m9 - pos.m6);
            q.Y = (float)Math.CopySign(q.Y, pos.m2 - pos.m8);
            q.Z = (float)Math.CopySign(q.Z, pos.m3 - pos.m4);
            return Matrix4x4.CreateScale(1) * Matrix4x4.CreateFromQuaternion(q) * Matrix4x4.CreateTranslation(new Vector3(pos.m3, pos.m7, pos.m11));
        }


        internal SteamVRController(OpenVRContext openVRContext, string ControllerName, uint deviceindex, Creality Creality, ulong Handle)
        {
            this.openVRContext = openVRContext;
            this.ControllerName = ControllerName;
            Logger.Log($"{ControllerName}  index: {deviceindex}");
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
            if(error != 0)
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
            var errorf = OVR.Input.GetAnalogActionData(GeneralmAxisHandle, ref GeneralmTriggerAixData, Analogsize, handle);
            if (errorf != 0)
            {
                Logger.Log(error.ToString());
            }
            var errorg = OVR.Input.GetAnalogActionData(GeneralmTriggerAixHandle, ref GeneralmTriggerAixData, Analogsize, handle);
            if (errorg != 0)
            {
                Logger.Log(error.ToString());
            }
            var errorh = OVR.Input.GetPoseActionData(GeneralmPosistionHandle, ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0f, ref GeneralmPosistionData, Possize, handle);
            if (errorh != 0)
            {
                Logger.Log(error.ToString());
            }
            if (PrimaryPressData.bActive)
            {
                Logger.Log("Yes");
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
