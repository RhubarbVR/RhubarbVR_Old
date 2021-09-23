using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Managers
{
	public class NetApiManager : IManager
	{
		private IEngine _engine;

		public string token = "";

		public bool islogin = false;

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
