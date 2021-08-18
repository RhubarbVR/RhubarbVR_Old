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
        public Sync<float> cullingDistance;

        public IPL.AudioBuffer iplOutputBuffer;

        private IPL._IPLBinauralEffect_t iplBinauralEffect;
        private IPL.AudioBuffer iplInputBuffer;

        public bool isNotCulled {
            get
            {
                float e = entity.globalPos().Distance(world.headTrans.Translation);
                return (e <= cullingDistance.value);
            }
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            audioSource = new SyncRef<IAudioSource>(this, newRefIds);
            spatialBlend = new Sync<float>(this, newRefIds);
            spatialBlend.value = 1f;
            cullingDistance = new Sync<float>(this, newRefIds);
            cullingDistance.value = 100f;
        }

        public override void onLoaded()
        {
            base.onLoaded();

            IPL.AudioBufferAllocate(engine.audioManager.iplContext, 1, engine.audioManager.AudioFrameSize, ref iplInputBuffer);

            IPL.AudioBufferAllocate(engine.audioManager.iplContext, 2, engine.audioManager.AudioFrameSize, ref iplOutputBuffer);

            var setings = new IPL.BinauralEffectSettings { hrtf = engine.audioManager.hrtf};

            var error =IPL.BinauralEffectCreate(engine.audioManager.iplContext, ref engine.audioManager.iplAudioSettings, ref setings, out iplBinauralEffect);
            if(error != IPL.Error.Success)
            {
                logger.Log("There is a prolem with BinauralEffectCreate");
            }
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
            if (iplBinauralEffect == default) return;
            if (audioSource.target == null) return;
            if (!audioSource.target.IsActive) return;
            if (audioSource.target.FrameInputBuffer == null) return;
            if (audioSource.target.FrameInputBuffer.Length < engine.audioManager.AudioFrameSizeInBytes) return;
            Matrix4x4.Decompose(world.headTrans, out Vector3 sc, out Quaternion ret, out Vector3 trans);
            var position = Vector3.Transform((entity.globalPos().ToSystemNumrics() - trans), Quaternion.Inverse(ret));
            var e = new IPL.Vector3(position.X, position.Y, position.Z);
            fixed (byte* ptr = audioSource.target.FrameInputBuffer)
            {
                var prams = new IPL.BinauralEffectParams { direction = e, interpolation = IPL.HRTFInterpolation.Nearest, spatialBlend = spatialBlend.value,hrtf = engine.audioManager.hrtf};
                IPL.AudioBufferDeinterleave(engine.audioManager.iplContext, (IntPtr)ptr, ref iplInputBuffer);
                IPL.BinauralEffectApply(iplBinauralEffect, ref prams, ref iplInputBuffer, ref iplOutputBuffer);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            IPL.AudioBufferFree(engine.audioManager.iplContext, ref iplInputBuffer);
        }

        public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public AudioOutput()
        {
        }
    }
}
