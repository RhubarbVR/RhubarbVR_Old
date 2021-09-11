using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using System.Numerics;
namespace RhubarbEngine.Input.Controllers
{
	public interface IKnucklesController : IController
	{
		bool Grip_Touch { get; }

		bool A_Pressed { get; }

		bool B_Pressed { get; }

		bool A_Touch { get; }

		bool B_Touch { get; }

		float Trigger { get; }

		bool Trigger_Touched { get; }

		bool Trigger_Click { get; }
		bool Joystick_Clicked { get; }
		Vector2f Touchpad_Aixs { get; }
		bool Touchpad_Touched { get; }
		bool Youchpad_Pressed { get; }
		Vector2f Joystick_Axis { get; }
		bool Joystick_Touched { get; }
		bool Grip_Click { get; }
		float Touchpad_Force { get; }

	}
}
