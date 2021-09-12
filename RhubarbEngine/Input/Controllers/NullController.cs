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
		string IController.ControllerName => "Null";

		Creality IController.Creality => Creality.None;

		bool IController.PrimaryPress => false;

		bool IController.TriggerTouching => false;

		bool IController.AxisTouching => false;

		bool IController.SystemPress => false;

		bool IController.MenuPress => false;

		bool IController.GrabPress => false;

		bool IController.SecondaryPress => false;

		Vector2f IController.Axis => Vector2f.Zero;

		float IController.TriggerAix => 0f;

		Matrix4x4 IController.Posistion => Matrix4x4.CreateScale(1);
	}
}
