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

        public SyncPlayback playback;

        private MemoryStream audioStream;

        public override void buildSyncObjs(bool newRefIds)
        {
            playback = new SyncPlayback(this, newRefIds);
            playback.Speed = 1f;
            playback.Looping = true;
            playback.stateChange += Playback_stateChange;
            Path = new Sync<string>(this, newRefIds);
            Path.value = "Quincas Moreira - Moskito.raw";
        }

        private double Playback_stateChange()
        {

            return (audioStream.Length / sizeof(float) / AudioManager.SamplingRate);
        }

        public unsafe override void onLoaded()
        {
            base.onLoaded();
            byte[] bytes = LoadRawAudio(Path.value);
            audioStream = new MemoryStream(bytes);
            IsActive = true;
            if (!playback.Playing)
            {
                playback.Resume();
                return;
            }
            playback.ClipLength = (audioStream.Length / sizeof(float) / AudioManager.SamplingRate);
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
            if (!playback.Playing)
            {
                frameInputBuffer = new byte[AudioManager.AudioFrameSizeInBytes];
                return;
            }
            audioStream.Position = ((int)(((int)((double)AudioManager.SamplingRate * (double)sizeof(float) * playback.Position)) / frameInputBuffer.Length)) * frameInputBuffer.Length;
            TimeSpan streamPositionTimeSpan = TimeSpan.FromSeconds((int)(audioStream.Position / sizeof(float) / AudioManager.SamplingRate));
            TimeSpan streamLengthTimeSpan = TimeSpan.FromSeconds((int)(audioStream.Length / sizeof(float) / AudioManager.SamplingRate));

            Console.WriteLine($"Position {playback.Position}   ");

            Console.WriteLine($"Stream position: {streamPositionTimeSpan.Minutes:D2}:{streamPositionTimeSpan.Seconds:D2} / {streamLengthTimeSpan.Minutes:D2}:{streamLengthTimeSpan.Seconds:D2}");
            Console.WriteLine($"Position { audioStream.Position}   ");

            audioStream.Read(frameInputBuffer, 0, frameInputBuffer.Length);
        }


        public RawAudioSource(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public RawAudioSource()
        {
        }
    }
}
