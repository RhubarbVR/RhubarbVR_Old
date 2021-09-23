using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.PlatformInfo;
using System.Threading;
namespace RhubarbEngine.Managers
{
	public class PlatformInfoManager : IManager
	{
		private IEngine _engine;

		private readonly OperatingSystem _os = Environment.OSVersion;

		public Platform platform = Platform.UNKNOWN;
		public string CPU { get; private set; } = "UNKNOWN";
		public string GPU { get; private set; } = "UNKNOWN";
		public long MemoryBytes { get; private set; } = -1L;

		public long VRAM_Bytes { get; private set; } = -1L;

		public DateTime startTime = DateTime.UtcNow;

		public DateTime Frame = DateTime.UtcNow;

		public ulong FrameCount = 0;

		public float FrameRate;

		public float AvrageFrameRate;

		public long previousFrameTicks = 0;

		public Stopwatch sw;

		public double deltaSeconds;

		public int ThreadCount;

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			sw = new Stopwatch();
			sw.Start();

			ThreadCount = Environment.ProcessorCount;

			if (OperatingSystem.IsOSPlatform("Linux"))
			{
				platform = Platform.Linux;
			}
			if (OperatingSystem.IsOSPlatform("macOS"))
			{
				platform = Platform.OSX;
			}
			if (OperatingSystem.IsOSPlatform("iOS"))
			{
				platform = Platform.iOS;
			}
			if (OperatingSystem.IsOSPlatform("Android"))
			{
				platform = Platform.Android;
			}
			if (OperatingSystem.IsOSPlatform("Windows"))
			{
				platform = Platform.Windows;
			}
			try
			{
				CPU = System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
			}
			catch (Exception e)
			{
				this._engine.Logger.Log("Failed to get CPU: " + e);
			}

			this._engine.Logger.Log("Platform: " + platform.ToString() + "/" + _os.Platform + " CPU: " + CPU + " RamBytes: " + MemoryBytes + " GPU: " + GPU + " VRAMBytes: " + VRAM_Bytes, true);
			return this;
		}
		long _currentFrameTicks;
        readonly float _vsync = 0;
        public void Update()
		{
			previousFrameTicks = _currentFrameTicks;
			_currentFrameTicks = sw.ElapsedTicks;
			deltaSeconds = (_currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
			FrameRate = 1f / (float)deltaSeconds;
			AvrageFrameRate = (FrameRate * 0.8f) + (AvrageFrameRate * 0.2f);
			if (_vsync != 0)
			{
				var sleepTime = (int)(((1 / _vsync) - deltaSeconds) * 1000);
				if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
		}
	}
}
