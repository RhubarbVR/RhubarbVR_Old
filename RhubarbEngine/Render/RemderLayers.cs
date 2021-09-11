using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render
{
	public enum RemderLayers
	{
		custom0 = 1,
		custom1 = 2,
		custom2 = 4,
		custom3 = 8,
		custom4 = 16,
		custom5 = 32,
		custom6 = 64,
		custom7 = 128,
		custom8 = 256,
		custom9 = 512,
		normal = 1024,
		overlay = 2048,
		privateOverlay = 4096,
		normal_overlay_privateOverlay = 7168,
		normal_overlay = 5120,
		custom0_custom1 = 3,
		custom2_custom3 = 12,
		custom4_custom5 = 48,
		custom4_custom5_custom6 = 112,
		custom7_custom8_custom9 = 896,
		all_customs = 1023,
		all = 8191,
	}
}
