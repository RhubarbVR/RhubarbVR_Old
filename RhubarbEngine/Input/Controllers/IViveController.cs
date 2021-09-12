using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
using System.Numerics;
namespace RhubarbEngine.Input.Controllers
{
	public interface IViveController : IController
	{
		bool Touchpad_Touch { get; }

		bool Touchpad_Click { get; }

		Vector2f Touchpad_Aixs { get; }

		bool Trigger_Hair { get; }

		bool Trigger_Click { get; }

		float Trigger { get; }

		bool Grip { get; }

		bool System { get; }

		bool App { get; }

	}
}
