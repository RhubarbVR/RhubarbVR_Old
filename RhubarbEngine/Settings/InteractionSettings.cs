using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuSettings;

namespace RhubarbEngine.Settings
{
	public class InteractionSettings : SettingsObject
	{
		[SettingsField("Laser Smoothing 10 default", "/Laser")]
		public float Smoothing = 10;

		[SettingsField("Laser Snaping Distance in 100ths 8 default", "/Laser")]
		public float SnapDistance = 8;

	}

}
