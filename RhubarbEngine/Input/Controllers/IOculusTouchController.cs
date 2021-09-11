using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using System.Numerics;
namespace RhubarbEngine.Input.Controllers
{
	public interface IOculusTouchController : IController
	{
		bool YB_Pressed { get; }
		bool XA_Pressed { get; }
		bool YB_Touched { get; }
		bool XA_Touched { get; }
		Vector2f Joystick_Aixs { get; }
		bool Joystick_Touch { get; }
		bool Joystick_Click { get; }
		bool Thumb_Rest_Touched { get; }
		float Grip_Aix { get; }
		bool Grip_Click { get; }
		float Trigger { get; }
		bool Trigger_Touch { get; }
		bool Trigger_Click { get; }

	}
}
