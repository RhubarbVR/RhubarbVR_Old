using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
using System.Numerics;
namespace RhubarbEngine.Input.Controllers
{
	public interface ICosmosController : IController
	{
		Vector2f Joystick_Aixs { get; }

		bool Joystick_Touched { get; }

		bool Joystick_Clicked { get; }

		float Trigger_Aix { get; }

		bool Trigger_Touched { get; }

		bool Trigger_Clicked { get; }

		bool Grip_Clicked { get; }

		bool Menu_Pressed { get; }

		bool AX_Pressed { get; }
		bool BY_Pressed { get; }
		bool Bumper { get; }

	}
}
