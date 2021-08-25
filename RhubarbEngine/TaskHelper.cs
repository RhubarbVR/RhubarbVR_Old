using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine
{
    public static class TaskHelper
    {
        public static async Task<bool> TimeOut(this Task task, int timeout)
        {
            return ((await Task.WhenAny(task, Task.Delay(timeout))) == task);
        }
    }
}
