using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RhubarbEngine
{
    public interface IUnitLogs
    {
        void CleanUP();
        void Log(string _log, bool _alwaysLog = false);
        bool WriteLog(string strMessage);
    }


    public class UnitLogs: IUnitLogs
	{
		private readonly IEngine _engine;

		public string logFile = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + ".txt";

		public string logDir = AppDomain.CurrentDomain.BaseDirectory + @"Logs";

		public FileStream objFilestream;

		public StreamWriter objStreamWriter;

		public UnitLogs(IEngine _engine)
		{
			this._engine = _engine;
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}
			objFilestream = new FileStream(Path.Combine(logDir, logFile), FileMode.OpenOrCreate, FileAccess.ReadWrite);
			objStreamWriter = new StreamWriter((Stream)objFilestream);
		}

		public void Log(string _log, bool _alwaysLog = false)
		{
			Console.WriteLine(string.Format("{0}: {1}", DateTime.Now, _log));
			if (_alwaysLog || _engine.Verbose)
			{
				WriteLog(string.Format("{0}: {1}", DateTime.Now, _log));
			}
		}
		public bool WriteLog(string strMessage)
		{
			try
			{
				objStreamWriter.WriteLine(strMessage);
				objStreamWriter.FlushAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void CleanUP()
		{
			try
			{
				objStreamWriter.Close();
				objFilestream.Close();
			}
			catch
			{
			}
		}
	}
}
