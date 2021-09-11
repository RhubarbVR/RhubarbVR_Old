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
                engine.initialize(_args);
                engine.startUpdateLoop();
            }
            catch (Exception e)
            {
                engine.logger.Log(e.ToString(), true);
            }

            try
            {
                engine.cleanUP();
            }
            catch (Exception e)
            {
                engine.logger.Log(e.ToString(), true);
                engine.logger.cleanUP();
            }
        }
    }
}