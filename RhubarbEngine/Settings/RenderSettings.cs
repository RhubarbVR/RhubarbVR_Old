using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;

namespace RhubarbEngine.Settings
{
    public class RenderSettings : SettingsObject
    {
        [SettingsField("Desktop Render Settings")]
        public DesktopRenderSettings DesktopRenderSettings = new DesktopRenderSettings();
    }


    public class DesktopRenderSettings : SettingsObject
    {
        [SettingsField("Render Resolution", "/resolution")]
        public int x = 1920;

        [SettingsField("Render Resolution", "/resolution")]
        public int y = 1080;

        [SettingsField("Use Window Resolution")]
        public bool auto = true;

        [SettingsField("Fov")]
        public float fov = 60f;
    }
}
