using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using CommandLine;
using RhubarbEngine.PlatformInfo;


namespace RhubarbEngine
{
    public class EngineInitializer
    {
        private Engine engine;

        public string intphase;
        public EngineInitializer(Engine _engine)
        {
            engine = _engine;
        }

        public void initializeManagers()
        {
            try
            {
                engine.logger.Log("Starting Managers");

                intphase = "Platform Info Manager";
                engine.logger.Log("Starting Platform Info Manager:");
                engine.platformInfo = new PlatformInfoManager();
                engine.platformInfo.initialize(engine);

                if(engine.platformInfo.platform != Platform.Android)
                {
                    intphase = "Window Manager";
                    engine.logger.Log("Starting Window Manager:");
                    engine.windowManager = new Managers.WindowManager();
                    engine.windowManager.initialize(engine);
                }
                else
                {

                }

                intphase = "World Manager";
                engine.logger.Log("Starting World Manager:");
                engine.worldManager = new WorldManager();
                engine.worldManager.initialize(engine);

            }
            catch (Exception _e)
            {
                engine.logger.Log("Failed at " + intphase + " Error: "+ _e);
            }

        }

        public void loadArguments(string[] _args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(_args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    if (o.verbose)
                    {
                        engine.verbose = true;
                    }
               });
        }
    }
}
