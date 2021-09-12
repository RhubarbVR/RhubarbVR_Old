using System;

using RhubarbEngine;

namespace RhubarbVR
{
    public static class Program
    {
        private static Engine Engine = new Engine();
        public static void Main(string[] _args)
        {
            try
            {
                Engine.Initialize(_args);
                Engine.StartUpdateLoop();
            }
            catch (Exception e)
            {
                Engine.logger.Log(e.ToString(), true);
            }

            try
            {
                Engine.CleanUP();
            }
            catch (Exception e)
            {
                Engine.logger.Log(e.ToString(), true);
                Engine.logger.CleanUP();
            }
        }
    }
}