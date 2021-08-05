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
using System.Runtime.InteropServices;

namespace RhubarbEngine.Components.Audio
{
    [Category(new string[] { "Audio" })]
    public class AudioOutput : Component
    {
        public SyncRef<IAudioSource> audioSource;

        public Sync<float> spatialBlend;
        public IPL.AudioBuffer iplOutputBuffer;

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
            IntPtr outputDataPtr = Marshal.AllocHGlobal(AudioManager.AudioFrameSizeInBytes * 2);

            iplOutputBuffer = new IPL.AudioBuffer
            {
                format = iplFormatStereo,
                numSamples = iplRenderingSettings.frameSize,
                interleavedBuffer = outputDataPtr
            };


            IPL.CreateBinauralEffect(iplBinauralRenderer, iplFormatMono, iplFormatStereo, out iplBinauralEffect);
        }

        public override void LoadListObject()
        {
            base.LoadListObject();
            try
            {
                if (!world.updateLists.audioOutputs.Contains(this))
                {
                    world.updateLists.audioOutputs.Add(this);
                }
            }
            catch { }
        }

        public override void RemoveListObject()
        {
            base.RemoveListObject();
            try
            {
                world.updateLists.audioOutputs.Remove(this);
            }
            catch { }
        }
        public unsafe void AudioUpdate()
        {
            if (audioSource.target == null) return;
            if (!audioSource.target.IsActive) return;
            Matrix4x4.Decompose(world.playerTrans, out Vector3 sc, out Quaternion ret, out Vector3 trans);
            var position = Vector3.Transform((entity.globalPos().ToSystemNumrics() - trans), Quaternion.Inverse(ret));
            var frameInputBuffer = audioSource.target.FrameInputBuffer;
            var e = new IPL.Vector3(position.X, position.Y, position.Z );

            fixed (byte* ptr = frameInputBuffer)
            { 
                iplInputBuffer.interleavedBuffer = (IntPtr)ptr;
            }
            IPL.ApplyBinauralEffect(iplBinauralEffect, iplBinauralRenderer, iplInputBuffer, e, IPL.HrtfInterpolation.Bilinear, spatialBlend.value, iplOutputBuffer);
        }

        public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public AudioOutput()
        {
        }
    }
}
