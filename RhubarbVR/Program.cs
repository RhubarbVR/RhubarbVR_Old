using System;
using System.IO;
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
                var tempFile = AppDomain.CurrentDomain.BaseDirectory + Guid.NewGuid().ToString() + ".tmp";
                using (File.Create(tempFile))
                { }
                File.Delete(tempFile);
                try
                {
                    engine.Initialize<BaseEngineInitializer, UnitLogs>(_args);
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
                    engine.Logger.Log("Failed To Close Game" + e.ToString(), true);
                    engine.Logger.CleanUP();
                }
            }
            catch(Exception e)
            {
                while(Console.ReadKey().Key != ConsoleKey.Escape)
                {
                    Console.WriteLine("An Error Was encountered Click Esc to close Error:" + e.ToString());
                }
            }

        }
    }
}