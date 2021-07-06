using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace RhubarbCloudApi.ClientTypes
{
    public class User
    {
        public PrivateData privatedata;

        public string uuid;

        public string username;

        public bool dev;

        public string avatarurl;

        public DateTime dateCreated;
        public DateTime bandate;
        public DateTime spectatorbandate;

        public JObject settings;

        public List<string> tags;

        public List<linkedAccountsI> otheracounts;
    }

    public class PrivateData
    {
        public List<string> friends;
    }
}
