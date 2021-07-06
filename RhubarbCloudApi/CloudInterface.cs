using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RhubarbCloudApi.ClientTypes;

namespace RhubarbCloudApi
{
    public class CloudInterface
    {
        public string dataPath;

        public static HttpClient client = new HttpClient();

        public Endpoints endpoints;

        public string startToken = "";

        public User user;

        public string token;

        public bool loggedIn = false;

        static async Task<ReturnData<T>> get<T>(string path) where T :  new()
        {
            ReturnData<T> data = new ReturnData<T>();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                string Json = await response.Content.ReadAsStringAsync();
                data.LoadJson(Json);
            }
            else
            {
                string ErrorMsg = await response.Content.ReadAsStringAsync();
                data.LoadError(ErrorMsg);
            }
            return data;
        }

        static async Task<ReturnData<T>> post<T>(string path, JObject obj) where T : new()
        {
            ReturnData<T> data = new ReturnData<T>();
            StringContent jsonString = new StringContent(obj.ToString(),System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(path, jsonString);
            if (response.IsSuccessStatusCode)
            {
                string Json = await response.Content.ReadAsStringAsync();
                data.LoadJson(Json);
            }
            else
            {
                string ErrorMsg = await response.Content.ReadAsStringAsync();
                data.LoadError(ErrorMsg);
            }
            return data;
        }

        public void loadRouts(Task<ReturnData<Endpoints>> task)
        {
            ReturnData<Endpoints> val = task.Result;
            if (val.error)
            {
                Console.WriteLine("Error LoadingRouts MSG:" + val.errorMsg);
            }
            else
            {
                endpoints = val.data;
                initializer();
            }
        }

        public void initializer()
        {
            Console.WriteLine("Loading set 1");
            if(startToken == "")
            {
                user = new User();
                user.username = "Anonymous";
                loggedIn = false;
            }
            else
            {
                loginWithToken(startToken);
            }
        }

        public void loginWithToken(string tokenl)
        {
            JObject value = new JObject();
            value.Add("token", tokenl);
            post<User>(endpoints.UserManagment + "/api/User/@me",value).ContinueWith((Task<ReturnData<User>> task) =>
            {
                Console.WriteLine("loggingin");
                ReturnData<User> data = task.Result;
                if (!data.error)
                {
                    user = data.data;
                    loggedIn = true;
                    token = tokenl;
                    Console.WriteLine("loggedin " + user.username);
                }
                else
                {
                    Console.WriteLine("loggedin error " + data.errorMsg);
                }
            });
        }

        public void startRouts()
        {
            Console.WriteLine("startingRouts");
            get<Endpoints>("https://api.rhubarbvr.net/api/routs").ContinueWith(loadRouts);
        }

        public CloudInterface(string _dataPath,string token)
        {
            dataPath = _dataPath;
            startToken = token;
            startRouts();
        }
    }
}
