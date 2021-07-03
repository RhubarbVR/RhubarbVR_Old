using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Managers;
using CommandLine;
using RhubarbEngine.PlatformInfo;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace RhubarbEngine
{
    public class EngineInitializer
    {
        private Engine engine;

        public string intphase;

        public bool Initialised = false;

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

                if (engine.platformInfo.platform != Platform.Android)
                {
                    intphase = "Window Manager";
                    engine.logger.Log("Starting Window Manager:");
                    engine.windowManager = new Managers.WindowManager();
                    engine.windowManager.initialize(engine);
                }
                else
                {

                }

                intphase = "Input Manager";
                engine.logger.Log("Starting Input Manager:");
                engine.inputManager = new Managers.InputManager();
                engine.inputManager.initialize(engine);

                intphase = "Render Manager";
                engine.logger.Log("Starting Render Manager:");
                engine.renderManager = new Managers.RenderManager();
                engine.renderManager.initialize(engine);

                intphase = "Net Api Manager";
                engine.logger.Log("Starting Net Api Manager:");
                engine.netApiManager = new Managers.NetApiManager();
                engine.netApiManager.initialize(engine);

                intphase = "World Manager";
                engine.logger.Log("Starting World Manager:");
                engine.worldManager = new WorldManager();
                engine.worldManager.initialize(engine);

                Initialised = true;
            }
            catch (Exception _e)
            {
                engine.logger.Log("Failed at " + intphase + " Error: " + _e);
            }

        }

        public IEnumerable<string> settings = new string[] { };

        public void loadArguments(string[] _args)
        {
            foreach(string arg in _args)
            {
                engine.logger.Log(arg, true);
            }
            Parser.Default.ParseArguments<CommandLineOptions>(_args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    if (o.verbose)
                    {
                        engine.verbose = true;
                    }
                    if (o.datapath != null)
                    {
                        engine.dataPath = o.datapath;
                    }
                    if (o.graphicsBackend != null)
                    {
                        engine.backend = o.graphicsBackend;
                    }
                    if (o.outputType != null)
                    {
                        engine.outputType = o.outputType;
                    }
                    if (o.settings != null)
                    {
                        settings = o.settings;
                    }
                });
        }
    }
}
