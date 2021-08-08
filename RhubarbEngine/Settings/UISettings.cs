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
        [SettingsField("Task Bar Curve 78 default","/VR")]
        public float TaskBarCurve = 78;
    }

}
