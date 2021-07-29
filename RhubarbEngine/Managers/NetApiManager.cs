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

        public bool islogin = false;

        public PrivateUser user = default;

        public PrivateStatus status = default;

        public event Action onlogin;

        public event Action onlogout;

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            engine.logger.Log("Starting Cloud Interface");

            string url = "https://api.rhubarbvr.net/";
            //string url = "http://localhost:44395/";

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
            if (text != "")
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

        public void setToken(string Token, PrivateUser User)
        {
            token = Token;
            user = User;
            Logger.Log("Welcome: " + user.Username, true);
            islogin = true;
            onlogin?.Invoke();
            try
            {
                var statuss = statusApi.StatusMeGet(token);
                status = statuss;
            }
            catch (Exception e)
            {
                status = new PrivateStatus();
            }
            if (status == null)
            {
                status = new PrivateStatus();
            }
            status.Versionkey = "test";
            status.Version = "pre alpha";
            status.Onlinelevel = 5;
            updateStatus();
        }

        public void updateStatus()
        {
            if (DateTime.UtcNow - status.Laststatus >= new TimeSpan(0, 2, 0))
            {
                StatusUpdate val = new StatusUpdate();
                val.Outputdevice = engine.outputType.ToString();
                val.Focusedsession = engine.worldManager?.focusedWorld?.SessionID?.value;
                val.Customstatus = "";
                val.Onlinelevel = 5;
                val.Versionkey = "test";
                val.Version = "pre alpha";
                val.Ismobile = false;

                statusApi.StatusStatusupdatePost(val, token);
                status.Laststatus = DateTime.UtcNow;
            }
        }

        public void login(string email, string password, bool rememberme)
        {
            var auth = authApi.AuthLoginPost(new LoginReg(password, email, rememberme));
            setToken(auth.Token, auth.User);
            if (rememberme)
            {
                File.WriteAllText(engine.dataPath + "\\auth.token", token);
            }
        }
        public void register(string email, string password, string username,DateTime birthday)
        {
            var auth = authApi.AuthRegisterPost(new RegesterReg(username, password,email, birthday));
            setToken(auth.Token, auth.User);
            File.WriteAllText(engine.dataPath + "\\auth.token", token);
        }

        public void logout()
        {
            token = "";
            user = null;
            islogin = false;
            onlogout?.Invoke();
            try
            {
                File.Delete(engine.dataPath + "\\auth.token");
            }
            catch { }
            statusApi.StatusClearstatusGet(token);
        }

        public void Update()
        {

        }

        public void Close()
        {
            statusApi.StatusClearstatusGet(token);
        }
    }
}
