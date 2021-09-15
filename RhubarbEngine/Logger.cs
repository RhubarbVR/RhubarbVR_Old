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
        public static Engine Engine { get; private set; }
        public static void Init(Engine _engine)
		{
            Logger.Engine = _engine;
		}

		public static void Log(string _log, bool _alwaysLog = false)
		{
			Engine.logger.Log(_log, _alwaysLog);
		}
	}
}
