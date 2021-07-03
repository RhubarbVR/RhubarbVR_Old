using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;

namespace RhubarbEngine.Settings
{
    public class MainSettingsObject : SettingsObject
    {
        [SettingsField("Render Settings")]
        public RenderSettings RenderSettings = new RenderSettings();
    }

}
