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
using System.Threading;

namespace RhubarbEngine.Components.Audio
{
    [Category(new string[] { "Audio" })]
    public class AudioOutput : Component
    {
        public SyncRef<IAudioSource> audioSource;

        public Sync<float> spatialBlend;

        private IntPtr iplBinauralEffect;
        private  IntPtr iplBinauralRenderer;
        private  IPL.AudioBuffer iplInputBuffer;


        public override void buildSyncObjs(bool newRefIds)
        {
            audioSource = new SyncRef<IAudioSource>(this, newRefIds);
            spatialBlend = new Sync<float>(this, newRefIds);
            spatialBlend.value = 1f;
        }

        public override void onLoaded()
        {
            base.onLoaded();

            


            var iplRenderingSettings = new IPL.RenderingSettings
            {
                samplingRate = AudioManager.SamplingRate,
                frameSize = AudioManager.AudioFrameSize
            };


            var hrtfParams = new IPL.HrtfParams
            {
                type = IPL.HrtfDatabaseType.Default
            };
            //Audio Formats
            IPL.CreateBinauralRenderer(engine.audioManager.iplContext, iplRenderingSettings, hrtfParams, out iplBinauralRenderer);

            var iplFormatMono = new IPL.AudioFormat
            {
                channelLayoutType = IPL.ChannelLayoutType.Speakers,
                channelLayout = IPL.ChannelLayout.Mono,
                channelOrder = IPL.ChannelOrder.Interleaved
            };
            var iplFormatStereo = new IPL.AudioFormat
            {
                channelLayoutType = IPL.ChannelLayoutType.Speakers,
                channelLayout = IPL.ChannelLayout.Stereo,
                channelOrder = IPL.ChannelOrder.Interleaved
            };

            iplInputBuffer = new IPL.AudioBuffer
            {
                format = iplFormatMono,
                numSamples = iplRenderingSettings.frameSize,
                interleavedBuffer = IntPtr.Zero //Will be assigned before use.
            };
            //Binaural Effect

            IPL.CreateBinauralEffect(iplBinauralRenderer, iplFormatMono, iplFormatStereo, out iplBinauralEffect);

        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            AudioUpdate();
        }

        public unsafe void AudioUpdate()
        {
            if (audioSource.target == null) return;
            if (!audioSource.target.IsActive) return;
            Console.WriteLine("Audio Update");
            Matrix4x4.Decompose(world.playerTrans, out Vector3 sc, out Quaternion ret, out Vector3 trans);
            var position = entity.globalPos().ToSystemNumrics() - trans;
            var frameInputBuffer = audioSource.target.FrameInputBuffer;
            var e = new IPL.Vector3(position.X * 100, position.Y * 100, position.Z * 100);
            fixed (byte* ptr = frameInputBuffer)
            { 
                iplInputBuffer.interleavedBuffer = (IntPtr)ptr;
            }
            Console.WriteLine(" X " + e.x.ToString() + " Y " + e.y.ToString() + " X " + e.z.ToString());
            IPL.ApplyBinauralEffect(iplBinauralEffect, iplBinauralRenderer, iplInputBuffer, e, IPL.HrtfInterpolation.Nearest, spatialBlend.value, engine.audioManager.iplOutputBuffer);
            
        }

        public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public AudioOutput()
        {
        }
    }
}
