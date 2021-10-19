using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace RhubarbEngine.Managers
{
    public class UserProfile
    {
        public string ownerId;

        public string etag;

        public string firstName;

        public string lastName;

        public string email;

        public IEnumerable<string> emails;

        public IEnumerable<string> openIds;

        public string UserName;

        public string displayName;

        public string rStudioUrl;

        public string summary;

        public string position;

        public string location;

        public string industry;

        public string company;

        public string profilePicureFileHandleId;

        public string url;

        public string teamName;

        public Dictionary<string, (string name, string concreteType)> preferences;

        public string createdOn;

        public UserProfile()
        {
            UserName = "anonymous";
            displayName = "Anonymous";
        }

        public UserProfile(string json)
        {
            var juser = JObject.Parse(json);
            ownerId = (string)juser["ownerId"];
            etag = (string)juser["etag"];
            firstName = (string)juser["firstName"];
            lastName = (string)juser["lastName"];
            email = (string)juser["email"];
            emails = juser["emails"].ToArray().Cast<string>();
            openIds = juser["openIds"].ToArray().Cast<string>();
            UserName = (string)juser["userName"];
            displayName = (string)juser["displayName"];
            rStudioUrl = (string)juser["rStudioUrl"];
            summary = (string)juser["summary"];
            position = (string)juser["position"];
            location = (string)juser["location"];
            industry = (string)juser["industry"];
            company = (string)juser["company"];
            profilePicureFileHandleId = (string)juser["profilePicureFileHandleId"];
            url = (string)juser["url"];
            teamName = (string)juser["teamName"];
            createdOn = (string)juser["createdOn"];

        }
    }


    public interface INetApiManager : IManager
    {
        bool Islogin { get; }
        string Token { get; set; }

        Task LoadUserData(string Token);
    }

    public class NetApiManager : INetApiManager
    {
        private const string RHUBARB_END_POINT = "https://matrix.rhubarbvr.net/";

		private IEngine _engine;

		public string Token { get; set; } = "";

		public bool Islogin { get; private set; } = false;

        public UserProfile userProfile = new UserProfile();

		public IManager Initialize(IEngine _engine)
		{
			this._engine = _engine;
			this._engine.Logger.Log("Starting Cloud Interface");
            if (_engine.LoginToken != null)
            {
                LoadUserData(_engine.LoginToken).ConfigureAwait(true);
            }
			return this;
		}

        public async Task LoadUserData(string Token)
        {
            var request = (HttpWebRequest)WebRequest.Create(RHUBARB_END_POINT+ "userProfile");
            request.Headers.Add("token",Token);
            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, Encoding.UTF8);
                var responseString = reader.ReadToEnd();
                if (responseString.Contains("username"))
                {
                    Islogin = true;
                    userProfile = new UserProfile(responseString);
                }
            }
        }

        public void Update()
        {

        }
	}
}
