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

        [SettingsField("UI Settings")]
        public UISettings UISettings = new UISettings();

        [SettingsField("Audio Settings")]
        public AudioSettings AudioSettings = new AudioSettings();

        [SettingsField("VR Settings")]
        public VRSettings VRSettings = new VRSettings();
    }

}
