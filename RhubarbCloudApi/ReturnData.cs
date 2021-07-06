using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbCloudApi
{
    public class ReturnData<T>
    {
        public T data;

        public bool error = false;

        public string errorMsg = "";

        public virtual void LoadJson(string json)
        {
            data = JsonConvert.DeserializeObject<T>(json);
        }

        public virtual void LoadError(string errorst)
        {
            error = true;
            errorMsg = errorst;
        }
    }
}
