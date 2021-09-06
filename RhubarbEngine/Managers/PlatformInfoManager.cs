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
        private Engine engine;

        private OperatingSystem os = Environment.OSVersion;

        public Platform platform = Platform.UNKNOWN;
        public string CPU { get; private set; } = "UNKNOWN";
        public string GPU { get; private set; } = "UNKNOWN";
        public long memoryBytes { get; private set; } = -1L;

        public long vRAM_Bytes { get; private set; } = -1L;

        public DateTime startTime = DateTime.UtcNow;

        public DateTime Frame = DateTime.UtcNow;

        public ulong FrameCount = 0;

        public float FrameRate;

        public float AvrageFrameRate;

        public long previousFrameTicks = 0;

        public Stopwatch sw;

        public double deltaSeconds;

        public int ThreadCount;

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
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
                engine.logger.Log("Failed to get CPU: " + e);
            }

            engine.logger.Log("Platform: " + platform.ToString() + "/" + os.Platform + " CPU: " + CPU + " RamBytes: " + memoryBytes + " GPU: " + GPU + " VRAMBytes: " + vRAM_Bytes, true);
            return this;
        }
        long currentFrameTicks;

        float vsync = 0f;
        public void Update()
        {
            previousFrameTicks = currentFrameTicks;
            currentFrameTicks = sw.ElapsedTicks;
            deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
            FrameRate = 1f / (float)deltaSeconds;
            AvrageFrameRate = (FrameRate)*0.8f + (AvrageFrameRate)*0.2f;
            if(vsync != 0)
            {
                var sleepTime = (int)(((1 / vsync) - deltaSeconds) * 1000);
                if (sleepTime > 0)
                    Thread.Sleep(sleepTime);
            }
        }
    }
}
