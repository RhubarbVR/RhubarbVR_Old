using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RhubarbEngine
{
    public static class Logger
    {
        private static Engine engine;
        public static Engine Engine { get { return engine; } }
        public static void init(Engine _engine)
        {
            engine = _engine;
        }

        public static void Log(string _log, bool _alwaysLog = false)
        {
            engine.logger.Log(_log, _alwaysLog);
        }
    }
}
