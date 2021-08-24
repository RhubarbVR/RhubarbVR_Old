using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuSettings;

namespace RhubarbEngine.Settings
{
    public class AudioSettings : SettingsObject
    {
        [SettingsField("User OpenAL or Naudio to output Audio", "/advanced")]
        public bool OpenAL = true;

        [SettingsField("Sampling Rate of 48000 or 44100 is recommended","/advanced")]
        public int SamplingRate = 48000;

        [SettingsField("Size of Buffer 2048 or 1024 is recommended", "/advanced")]
        public int AudioFrameSize = 2048;

        [SettingsField("Can reduce popping but adds latency to audio 2 is recommended", "/advanced")]
        public int BufferCount = 3;

    }

}
