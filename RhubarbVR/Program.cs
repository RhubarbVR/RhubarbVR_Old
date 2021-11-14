using System;

using RhubarbEngine;

namespace RhubarbVR
{
    public static class Program
    {
        public static Engine engine = new();
        public static void Main(string[] _args)
        {
            try
            {
                engine.Initialize<BaseEngineInitializer,UnitLogs>(_args);
                engine.StartUpdateLoop();
            }
            catch (Exception e)
            {
                engine.Logger.Log(e.ToString(), true);
            }
            engine.Logger.Log("Closing Game", true);
            try
            {
                engine.CleanUP();
            }
            catch (Exception e)
            {
                engine.Logger.Log("Failed To Close Game"+e.ToString(), true);
                engine.Logger.CleanUP();
            }
        }
    }
}