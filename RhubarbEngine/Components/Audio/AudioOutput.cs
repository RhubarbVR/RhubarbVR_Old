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
using OpenAL;

namespace RhubarbEngine.Components.Audio
{
	[Category(new string[] { "Audio" })]
	public unsafe class AudioOutput : Component
	{
		public SyncRef<IAudioSource> audioSource;

		public Sync<float> cullingDistance;

        public Sync<float> velocityMultiplier;

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
            audioSource.Changed += AudioSource_Changed;
            cullingDistance = new Sync<float>(this, newRefIds)
            {
                Value = 100f
            };
            velocityMultiplier = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
        }

        private void AudioSource_Changed(IChangeable obj)
        {
            if (audioSource.Target is null)
            {
                return;
            }

            audioSource.Target.Update += UpdateAudio;
        }

        public void UpdateAudio()
        {
            if (IsNotCulled)
            {
                if (!audioSource.Target.IsActive)
                {
                    return;
                }

                var data = audioSource.Target.FrameInputBuffer;
                if (_stream.CanWrite && data != null)
                {
                    //  if you want to use BeginWrite here instead you need to copy the _readBuffer to avoid race conditions reader/writing the same buffer asynchronously
                    //  alternatively you can use multiple buffers to avoid such race conditions
                    _stream.Write(data, 0, data.Length);
                }
            }
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if(_stream is null)
            {
                return;
            }
            var e = Entity.GlobalTrans();
            _stream.Velocity = (_stream.ALPosition - e.Translation) * ((float)Engine.PlatformInfo.DeltaSeconds * velocityMultiplier.Value);
            _stream.ALPosition = e.Translation;
        }

        private PlaybackStream _stream;

        public unsafe override void OnLoaded()
		{
			base.OnLoaded();
            if (Engine.Audio)
            {
                _stream = Engine.AudioManager.Device.OpenStream((uint)Engine.AudioManager.SamplingRate, OpenALAudioFormat.Mono16Bit);
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
