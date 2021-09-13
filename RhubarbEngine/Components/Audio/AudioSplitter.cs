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
using SteamAudio;
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
			public bool IsActive => isConpatable();

			public bool isConpatable()
			{
				if (audioParent != null)
					return false;
				return active.Value && audioParent.isConpatable() && (getPos() < getParentCount());
			}

			public int getPos()
			{
				if (audioParent == null)
					return -1;
				return audioParent.outputs.GetIndexOf(this);
			}
			public int getParentCount()
			{
				if (audioParent == null)
					return -1;
				return audioParent.outputs.Count();
			}
			public int ChannelCount => 1;

			public AudioSplitter audioParent;

			public byte[] FrameInputBuffer => audioParent.getData(getPos());

			public override void OnLoaded()
			{
				base.OnLoaded();
				try
				{
					audioParent = (AudioSplitter)(parent.Parent);
				}
				catch
				{

				}
			}

			public override void BuildSyncObjs(bool newRefIds)
			{
				base.BuildSyncObjs(newRefIds);
				active = new Sync<bool>(this, newRefIds);
				active.Value = true;
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

		public byte[] getData(int channelminsone)
		{
			if (channelminsone == 0)
			{
				data = audioSource.Target.FrameInputBuffer;
			}
			var returnData = new byte[Engine.audioManager.AudioFrameSizeInBytes];
			int index = 0;
			for (int i = 0; i < data.Length; i += 8)
			{
				if (channelminsone == ((i / 8) % audioSource.Target.ChannelCount))
				{
					for (int e = 0; e < 8; e++)
					{
						returnData[e + index] = data[e + i];
					}
					index += 8;
				}
			}
			return returnData;
		}


		public bool isConpatable()
		{
			if (audioSource.Target == null)
				return false;
			return (outputs.Count() == audioSource.Target.ChannelCount);
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
