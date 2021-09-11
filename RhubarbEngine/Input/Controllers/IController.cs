using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using System.Numerics;
namespace RhubarbEngine.Input.Controllers
{
	public interface IController
	{
		string ControllerName { get; }
		Creality Creality { get; }
		bool PrimaryPress { get; }
		bool TriggerTouching { get; }
		bool AxisTouching { get; }
		bool SystemPress { get; }
		bool MenuPress { get; }
		bool GrabPress { get; }
		bool SecondaryPress { get; }
		Vector2f Axis { get; }
		float TriggerAix { get; }
		Matrix4x4 Posistion { get; }
	}
}
