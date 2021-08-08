using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;

namespace RhubarbEngine.Settings
{
    public class VRSettings : SettingsObject
    {
        [SettingsField("Makes Rhubarb start SteamVR on start")]
        public bool StartInVR = true;

        [SettingsField("Makes Rhubarb run as Overlay in SteamVR")]
        public bool StartAsOverlay = false;
    }

}
