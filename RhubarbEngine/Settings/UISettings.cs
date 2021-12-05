using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuSettings;

namespace RhubarbEngine.Settings
{
	public class UISettings : SettingsObject
	{
		[SettingsField("Task Bar Curve 80 default", "/VR")]
		public float TaskBarCurve = 80;

		[SettingsField("KeyBoard Curve 50 default", "/VR")]
		public float KeyBoardCurve = 50;

        [SettingsField("Rounding in px 3 default", "/Theme")]
        public float Rounding = 3;

        [SettingsField("", "/Theme")]
        public ThemeColor Color = ThemeColor.Dark;
    }

}
