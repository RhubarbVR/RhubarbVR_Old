using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.Input;
using RhubarbEngine.Input.Controllers;

namespace RhubarbEngine.VirtualReality.OpenVR.Controllers
{
    internal class SteamVRController : IController
    {
        public OpenVRContext openVRContext;

        public string ControllerName;
        string IController.ControllerName => ControllerName;

        public Creality Creality;
        Creality IController.Creality => Creality;

        bool IController.PrimaryPress => throw new NotImplementedException();

        bool IController.TriggerTouching => throw new NotImplementedException();

        bool IController.AxisTouching => throw new NotImplementedException();

        bool IController.SystemPress => throw new NotImplementedException();

        bool IController.MenuPress => throw new NotImplementedException();

        bool IController.GrabPress => throw new NotImplementedException();

        bool IController.SecondaryPress => throw new NotImplementedException();

        bool IController.TriggerPress => throw new NotImplementedException();

        Vector2f IController.Axis => throw new NotImplementedException();

        float IController.TriggerAix => throw new NotImplementedException();

        Matrix4x4 IController.Posistion => throw new NotImplementedException();

        internal SteamVRController(OpenVRContext openVRContext,string ControllerName, Creality creality)
        {
            this.openVRContext = openVRContext;
            this.Creality = creality;
            this.ControllerName = ControllerName;
        }
    }
}
