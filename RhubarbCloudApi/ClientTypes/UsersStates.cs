using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbCloudApi.ClientTypes
{
    public class UsersStates
    {
        public string UserUUID;
        public string Status;
        public Status StatusEnum;
        public Session CurrentSession;

    }
}
