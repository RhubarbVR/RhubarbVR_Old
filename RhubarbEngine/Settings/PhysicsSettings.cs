using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;

namespace RhubarbEngine.Settings
{
    public class PhysicsSettings : SettingsObject
    {
        [SettingsField("ThreadCount", "/advanced")]
        public int ThreadCount = -1;

    }

}
