using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RhubarbEngine
{
	public static class TaskHelper
	{
		public static async Task<bool> TimeOut(this Task task, int timeout)
		{
			var source = new CancellationTokenSource();
			var check = await Task.WhenAny(task, Task.Delay(timeout, source.Token));
			var retunval = check == task;
			if (retunval)
			{
				source.Cancel();
			}
			return retunval;
		}
	}
}
