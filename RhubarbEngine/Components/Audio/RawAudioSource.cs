using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using System.Numerics;
using SteamAudio;
using RhubarbEngine.Managers;
using System.IO;

namespace RhubarbEngine.Components.Audio
{
    [Category(new string[] { "Audio" })]
    public class RawAudioSource : Component, IAudioSource
    {
        public bool IsActive { get; set; }

        public int ChannelCount { get; private set; }

        public byte[] FrameInputBuffer => frameInputBuffer;

        byte[] frameInputBuffer = new byte[AudioManager.AudioFrameSizeInBytes];


        public Sync<string> Path;

        private MemoryStream audioStream;

        public override void buildSyncObjs(bool newRefIds)
        {

                Path = new Sync<string>(this, newRefIds);
            Path.value = "Quincas Moreira - Moskito.raw";
        }

        public unsafe override void onLoaded()
        {
            base.onLoaded();
            byte[] bytes = LoadRawAudio(Path.value);
            audioStream = new MemoryStream(bytes);
            IsActive = true;
        }
        internal static byte[] LoadRawAudio(string rawAudioPath)
        {
            if (string.IsNullOrWhiteSpace(rawAudioPath))
            {
                throw new ArgumentException($"Provide a path to a raw audio file in command line arguments to play it.");
            }

            if (!File.Exists(rawAudioPath))
            {
                throw new ArgumentException($"Invalid audio path: '{rawAudioPath}'.");
            }

            return File.ReadAllBytes(rawAudioPath);
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (!IsActive) return;

            TimeSpan streamPositionTimeSpan = TimeSpan.FromSeconds((int)(audioStream.Position / sizeof(float) / AudioManager.SamplingRate));
            TimeSpan streamLengthTimeSpan = TimeSpan.FromSeconds((int)(audioStream.Length / sizeof(float) / AudioManager.SamplingRate));

            Console.WriteLine($"Stream position: {streamPositionTimeSpan.Minutes:D2}:{streamPositionTimeSpan.Seconds:D2} / {streamLengthTimeSpan.Minutes:D2}:{streamLengthTimeSpan.Seconds:D2}");

            int bytesRead = audioStream.Read(frameInputBuffer, 0, frameInputBuffer.Length);

            //Loop the audio on stream end.
            if (bytesRead < frameInputBuffer.Length)
            {
                audioStream.Position = 0;

                audioStream.Read(frameInputBuffer, 0, frameInputBuffer.Length - bytesRead);
            }
        }

            public RawAudioSource(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public RawAudioSource()
        {
        }
    }
}
