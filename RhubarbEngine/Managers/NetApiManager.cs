using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Managers
{
    public interface INetApiManager : IManager
    {
        bool Islogin { get; }
        string Token { get; set; }
    }

    public class NetApiManager : INetApiManager
    {
		private IEngine _engine;

		public string Token { get; set; } = "";

		public bool Islogin { get; private set; } = false;

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			this._engine.Logger.Log("Starting Cloud Interface");

			return this;
		}
        public void Update()
        {

        }
	}
}
