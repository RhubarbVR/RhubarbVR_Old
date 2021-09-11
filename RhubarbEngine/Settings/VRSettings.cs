using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.VirtualReality;

using RhuSettings;

namespace RhubarbEngine.Settings
{
	public class VRSettings : SettingsObject
	{
		[SettingsField("Makes Rhubarb start SteamVR on start")]
		public bool StartInVR = true;


		[SettingsField("Out Put Eye")]
		public MirrorTextureEyeSource renderEye = MirrorTextureEyeSource.BothEyes;

		[SettingsField("Makes Rhubarb run as Overlay in SteamVR")]
		public bool StartAsOverlay = false;
	}

}
