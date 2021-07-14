using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Model;
namespace RhubarbEngine.Managers
{
    public class NetApiManager : IManager
    {
        private Engine engine;

        public AuthApi authApi;

        public AdminApi adminApi;

        public StatusApi statusApi;

        public SessionApi sessionApi;

        public UsersApi usersApi;

        public string token = "";

        public PrivateUser user;

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            engine.logger.Log("Starting Cloud Interface");
            
            string url = "https://api.rhubarbvr.net/";
            authApi = new AuthApi(url);
            adminApi = new AdminApi(url);
            statusApi = new StatusApi(url);
            sessionApi = new SessionApi(url);
            usersApi = new UsersApi(url);

            string text = "";
            try
            {
                text = File.ReadAllText(engine.dataPath + "\\auth.token");
            }
            catch
            {
            }
            if (token != "")
            {
                token = text;
                autologin();
            }

            return this;    
        }

        public void autologin()
        {
            var auth = usersApi.UsersMeGet(token);
            setToken(token, auth);
        }

        public void setToken(string Token,PrivateUser User)
        {
            token = Token;
            user = User;
        }

        public void login(string email,string password,bool rememberme)
        {
            var auth = authApi.AuthLoginPost(new LoginReg(password, email, rememberme));
            setToken(auth.Token,auth.User);
            if (rememberme)
            {
                File.WriteAllText(engine.dataPath + "\\auth.token", token);
            }
        }

        public void logout()
        {
            token = "";
            user = null;
            try
            {
                File.Delete(engine.dataPath + "\\auth.token");
            }
            catch { }
        }

        public void Update()
        {

        }
    }
}
