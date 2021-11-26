using RNumerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Input.Controllers
{
	public class NullController : IController
	{
        public Matrix4x4 PosistionWithOffset
        {
            get
            {
               return Matrix4x4.CreateScale(1);
            }
        }

        string IController.ControllerName
        {
            get
            {
                return "Null";
            }
        }

        Creality IController.Creality
        {
            get
            {
                return Creality.None;
            }
        }

        bool IController.PrimaryPress
        {
            get
            {
                return false;
            }
        }

        bool IController.TriggerTouching
        {
            get
            {
                return false;
            }
        }

        bool IController.AxisTouching
        {
            get
            {
                return false;
            }
        }

        bool IController.SystemPress
        {
            get
            {
                return false;
            }
        }

        bool IController.MenuPress
        {
            get
            {
                return false;
            }
        }

        bool IController.GrabPress
        {
            get
            {
                return false;
            }
        }

        bool IController.SecondaryPress
        {
            get
            {
                return false;
            }
        }

        Vector2f IController.Axis
        {
            get
            {
                return Vector2f.Zero;
            }
        }

        float IController.TriggerAix
        {
            get
            {
                return 0f;
            }
        }

        Matrix4x4 IController.Posistion
        {
            get
            {
                return Matrix4x4.CreateScale(1);
            }
        }
    }
}
