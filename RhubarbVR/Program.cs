using System;

using RhubarbEngine;

namespace RhubarbVR
{
    public static class Program
    {
        public static Engine engine = new Engine();
        public static void Main(string[] _args)
        {
            try
            {
                engine.Initialize(_args);
                engine.StartUpdateLoop();
            }
            catch (Exception e)
            {
                engine.logger.Log(e.ToString(), true);
            }

            try
            {
                engine.CleanUP();
            }
            catch (Exception e)
            {
                engine.logger.Log(e.ToString(), true);
                engine.logger.CleanUP();
            }
        }
    }
}