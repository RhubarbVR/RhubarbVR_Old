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
		public RenderSettings RenderSettings = new();

		[SettingsField("Physics Settings")]
		public PhysicsSettings PhysicsSettings = new();

		[SettingsField("UI Settings")]
		public UISettings UISettings = new();

		[SettingsField("Audio Settings")]
		public AudioSettings AudioSettings = new();

		[SettingsField("VR Settings")]
		public VRSettings VRSettings = new();

		[SettingsField("Interaction Settings")]
		public InteractionSettings InteractionSettings = new();
	}

}
