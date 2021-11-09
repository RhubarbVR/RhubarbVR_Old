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

namespace RhubarbEngine.Components.Audio
{
	[Category(new string[] { "Audio" })]
	public class AudioOutput : Component
	{
		public SyncRef<IAudioSource> audioSource;

		public Sync<float> spatialBlend;
		public Sync<float> cullingDistance;


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

		public override void OnLoaded()
		{
			base.OnLoaded();

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
