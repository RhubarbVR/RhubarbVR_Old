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
	public class AudioSplitter : Component
	{
		public class AudioSplits : Worker, IAudioSource
		{
			public Sync<bool> active;
            public bool IsActive
            {
                get
                {
                    return IsConpatable();
                }
            }

            public bool IsConpatable()
			{
                return !(audioParent != null || !active.Value || !audioParent.IsConpatable() || GetPos() >= GetParentCount());
            }

            public int GetPos()
			{
                return audioParent == null ? -1 : audioParent.outputs.GetIndexOf(this);
            }
            public int GetParentCount()
			{
                return audioParent == null ? -1 : audioParent.outputs.Count();
            }
            public int ChannelCount
            {
                get
                {
                    return 1;
                }
            }
            [NoSave]
            [NoShow]
            [NoSync]
            [NonSerialized]
            public AudioSplitter audioParent;

            public event Action Update;

            public byte[] FrameInputBuffer
            {
                get
                {
                    return audioParent.GetData(GetPos());
                }
            }

            public override void OnLoaded()
			{
				base.OnLoaded();
				try
				{
					audioParent = (AudioSplitter)parent.Parent;
				}
				catch
				{

				}
			}

			public override void BuildSyncObjs(bool newRefIds)
			{
				base.BuildSyncObjs(newRefIds);
                active = new Sync<bool>(this, newRefIds)
                {
                    Value = true
                };
            }
			public AudioSplits(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
			{

			}
			public AudioSplits()
			{
			}
		}
		public SyncObjList<AudioSplits> outputs;

		public SyncRef<IAudioSource> audioSource;

		public byte[] data;

		public byte[] GetData(int channelminsone)
		{
			if (channelminsone == 0)
			{
				data = audioSource.Target.FrameInputBuffer;
			}
			var returnData = new byte[Engine.AudioManager.AudioFrameSizeInBytes];
			var index = 0;
			for (var i = 0; i < data.Length; i += 8)
			{
                if (channelminsone == (i / 8 % audioSource.Target.ChannelCount))
				{
					for (var e = 0; e < 8; e++)
					{
						returnData[e + index] = data[e + i];
					}
					index += 8;
				}
			}
			return returnData;
		}


        public bool IsConpatable()
        {

            return audioSource.Target != null && outputs.Count() == audioSource.Target.ChannelCount;
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			outputs = new SyncObjList<AudioSplits>(this, newRefIds);
			audioSource = new SyncRef<IAudioSource>(this, newRefIds);
		}

		public AudioSplitter(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public AudioSplitter()
		{
		}
	}
}
