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
    public interface IPlatformInfoManager : IManager
    {
        string CPU { get; }
        string GPU { get; }
        double MemoryBytes { get; }
        double VRAM_Bytes { get; }
        Platform Platform { get; }
        int ThreadCount { get; }
        float AvrageFrameRate { get; }
        float FrameRate { get; }
        ulong FrameCount { get; }
        double DeltaSeconds { get; }
        DateTime Frame { get; set; }
        DateTime StartTime { get; set; }
        TimeSpan Elapsed { get; }

        void LoadGpuInfo(string name, double vrma);
        void NextFrame();
    }


    public class PlatformInfoManager : IPlatformInfoManager
    {
		private IEngine _engine;

		private readonly OperatingSystem _os = Environment.OSVersion;

		public Platform platform = Platform.UNKNOWN;
        public Platform Platform { get { return platform; } }

        public string CPU { get; private set; } = "UNKNOWN";
		public string GPU { get; private set; } = "UNKNOWN";
		public double MemoryBytes { get; private set; } = -1L;

		public double VRAM_Bytes { get; private set; } = -1L;

		public DateTime StartTime { get; set; } = DateTime.UtcNow;

		public DateTime Frame { get; set; } = DateTime.UtcNow;

		public ulong FrameCount { get; private set; }

        public float FrameRate { get; private set; }

        public float AvrageFrameRate { get; private set; }

        public long previousFrameTicks = 0;

		public Stopwatch sw;

		public double DeltaSeconds { get; private set; }

        public int ThreadCount { get; private set; }

        public TimeSpan Elapsed
        {
            get
            {
                return sw.Elapsed;
            }
        }

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
				CPU = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
			}
			catch (Exception e)
			{
				this._engine.Logger.Log("Failed to get CPU: " + e);
			}
            var memoryMetrics = MemoryMetricsClient.GetMetrics();
            MemoryBytes = memoryMetrics.Total;



            this._engine.Logger.Log("Platform: " + platform.ToString() + "/" + _os.Platform + " CPU: " + CPU + " Ram: " + MemoryBytes, true);
			return this;
		}
		long _currentFrameTicks;
        readonly float _vsync = 0;

        public void LoadGpuInfo(string name,double vrma)
        {
            GPU = name;
            VRAM_Bytes = vrma;
            this._engine.Logger.Log("GPU: " + GPU + " VRAM: " + VRAM_Bytes, true);
        }

        public void Update()
		{
			previousFrameTicks = _currentFrameTicks;
			_currentFrameTicks = sw.ElapsedTicks;
			DeltaSeconds = (_currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
			FrameRate = 1f / (float)DeltaSeconds;
			AvrageFrameRate = (FrameRate * 0.8f) + (AvrageFrameRate * 0.2f);
			if (_vsync != 0)
			{
				var sleepTime = (int)(((1 / _vsync) - DeltaSeconds) * 1000);
				if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
		}

        public void NextFrame()
        {
            FrameCount++;
        }
    }
}
