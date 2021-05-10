using System;
using RhubarbEngine;

namespace RhubarbVR
{
    public static class Program
    {
       public static Engine engine = new Engine();
        public static void Main(string[] _args)
        {
            engine.initialize(_args);
            engine.startUpdateLoop();
            engine.cleanUP();
            return;
        }
    }
}
