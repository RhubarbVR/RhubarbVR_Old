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
	public unsafe class AudioOutput : Component , IVelocityReqwest
	{
		public SyncRef<IAudioSource> audioSource;

		public Sync<float> cullingDistance;

        public Sync<float> velocityMultiplier;

        public Sync<float> gain;

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
            gain = new Sync<float>(this, newRefIds)
            {
                Value = 100f
            };
            gain.Changed += GainChange;
        }

        private void GainChange(IChangeable obj)
        {
            if(_stream is null)
            {
                return;
            }
            _stream.Gain = gain.Value;
        }

        private void AudioSource_Changed(IChangeable obj)
        {
            if (audioSource.Target is null)
            {
                return;
            }
            audioSource.Target.Update += UpdateAudio;
            audioSource.Target.Reload += Reload;
            Reload();
        }

        private void Reload()
        {
            UnloadAudio();
            LoadAudio();
        }

        public void UpdateAudio()
        {
            if (_stream is null)
            {
                return;
            }
            if (IsNotCulled)
            {
                if (!audioSource.Target.IsActive)
                {
                    return;
                }

                var data = audioSource.Target.FrameInputBuffer;
                if (_stream.CanWrite && data != null)
                {
                    _stream.Write(data, 0, data.Length);
                }
            }
        }

        private PlaybackStream _stream;

        public unsafe override void OnLoaded()
		{
			base.OnLoaded();
            Entity.GlobalTransformChange += Entity_GlobalTransformChange;
            Engine.AudioManager.PlayBackChanged += Reload;
        }

        public void LoadAudio()
        {
            if (Engine.Audio && audioSource.Target is null)
            {
                return;
            }

            var count = audioSource.Target.ChannelCount;
            if (count == 1)
            {
                _stream = Engine.AudioManager.Device.OpenStream((uint)Engine.AudioManager.SamplingRate, OpenALAudioFormat.Mono16Bit);
                _stream.Velocity = Entity.Velocity * velocityMultiplier.Value;
                _stream.ALPosition = Entity.GlobalTrans().Translation;
            }
            else if(count == 2)
            {
                _stream = Engine.AudioManager.Device.OpenStream((uint)Engine.AudioManager.SamplingRate, OpenALAudioFormat.Stereo16Bit);
                _stream.Velocity = Entity.Velocity * velocityMultiplier.Value;
                _stream.ALPosition = Entity.GlobalTrans().Translation;
            }
            else
            {
                Engine.Logger.Log($"Unsported Channel Count{count}", true);
            }
        }

        public void UnloadAudio()
        {
            if(_stream is null)
            {
                return;
            }
            _stream.Dispose();
            _stream = null;
        }

        private void Entity_GlobalTransformChange(Matrix4x4 obj)
        {
            if (_stream is null)
            {
                return;
            }
            _stream.Velocity = Entity.Velocity * velocityMultiplier.Value;
            _stream.ALPosition = obj.Translation;
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
            UnloadAudio();
        }

        public AudioOutput(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public AudioOutput()
		{
		}
	}
}
