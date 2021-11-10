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
using RNumerics;
using System.Numerics;
using RhubarbEngine.Managers;
using System.Threading;
using System.Runtime.InteropServices;
using RVRAudioNative;

namespace RhubarbEngine.Components.Audio
{
	[Category(new string[] { "Audio" })]
	public unsafe class AudioOutput : Component
	{
		public SyncRef<IAudioSource> audioSource;

		public Sync<float> spatialBlend;
		public Sync<float> cullingDistance;

        private rvrAudioSource* _audioSource;

		public bool IsNotCulled
		{
			get
			{
				if (audioSource.Target == null)
                {
                    return false;
                }

                if (!audioSource.Target.IsActive)
                {
                    return false;
                }

                if (audioSource.Target.ChannelCount != 1)
                {
                    return false;
                }

                var e = Entity.GlobalPos().Distance(World.HeadTrans.Translation);
				return e <= cullingDistance.Value;
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			audioSource = new SyncRef<IAudioSource>(this, newRefIds);
            spatialBlend = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
            cullingDistance = new Sync<float>(this, newRefIds)
            {
                Value = 100f
            };
        }

        public void UpdateAudio()
        {
            if (!IsNotCulled)
            {
                if (!NativeAudio.rvrAudioSourceIsPlaying(_audioSource))
                {
                    NativeAudio.rvrAudioSourcePlay(_audioSource); 
                }
                var freebuff = NativeAudio.rvrAudioSourceGetFreeBuffer(_audioSource);
                if ((IntPtr)freebuff != IntPtr.Zero)
                {
                    if (audioSource.Target != null)
                    {
                        var buffer = audioSource.Target.FrameInputBuffer;
                        fixed (byte* e = buffer)
                        {
                            NativeAudio.rvrAudioSourceQueueBuffer(_audioSource, freebuff, (short*)e, buffer.Length,(BufferType)2);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Audio Comp Culled");
                NativeAudio.rvrAudioSourcePause(_audioSource);
            }
        }

		public unsafe override void OnLoaded()
		{
			base.OnLoaded();
            if (Engine.Audio)
            {
                Console.WriteLine("Audio Comp Added");
               _audioSource = NativeAudio.rvrAudioSourceCreate(Engine.AudioManager.DefaultListener, Engine.AudioManager.SamplingRate, 0, true, true, true, true, 1, 1);
               var buf1 = NativeAudio.rvrAudioBufferCreate(Engine.AudioManager.AudioFrameSizeInBytes);
               var buf2 = NativeAudio.rvrAudioBufferCreate(Engine.AudioManager.AudioFrameSizeInBytes);
                NativeAudio.rvrAudioSourceSetBuffer(_audioSource, buf1);
                NativeAudio.rvrAudioSourceSetBuffer(_audioSource, buf2);
                var clear = new byte[Engine.AudioManager.AudioFrameSizeInBytes];
                fixed (byte* e = clear)
                {
                    NativeAudio.rvrAudioSourceQueueBuffer(_audioSource, buf1, (short*)e, Engine.AudioManager.AudioFrameSizeInBytes, (BufferType)1);
                    NativeAudio.rvrAudioSourceQueueBuffer(_audioSource, buf2, (short*)e, Engine.AudioManager.AudioFrameSizeInBytes, (BufferType)2);

                }
                Console.WriteLine("Audio Comp Loaded");
            }
        }

		public override void LoadListObject()
		{
			base.LoadListObject();
			try
			{
				World.updateLists.audioOutputs.SafeAdd(this);
			}
			catch { }
		}

		public override void RemoveListObject()
		{
			base.RemoveListObject();
			try
			{
				World.updateLists.audioOutputs.Remove(this);
			}
			catch { }
		}
		public unsafe void AudioUpdate()
		{
            if (!audioSource.Target.IsActive)
            {
                return;
            }

            var data = audioSource.Target.FrameInputBuffer;
			if (data == null)
            {
                return;
            }
		}

        public override void Dispose()
		{
            base.Dispose();
        }

        public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public AudioOutput()
		{
		}
	}
}
