using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace twbot
{
    class TribalWars
    {
        private Browser _m;
        private string _host;
        private string _user;
        private string _password;
        private bool _loggedIn;

        // TODO: expand and complete
        public enum View
        {
            MAIN,
            VILLAGES,
            MAP
        };

        public TribalWars(string ip, Browser m = null)
        {
            _m = m ?? new Browser();
            _host = ip;
            _user = "";
            _password = "";
            _loggedIn = false;
        }


        public bool login(string name, string password)
        {
            // TODO: urlencode
            _user = name;
            _password = password;

            // build post data
            string data = new StringBuilder("").AppendFormat("user={0}&clear=true&password={1}", _user, _password).ToString();
            

            // login
            int status = _m.post(Browser.construct(_host, "index.php", "action=login"), "user="+_user+"&password="+_password);
            if (status == 302)
            { // expected redirection , login failure otherwise
                string location = _m.getRedirect();

                Console.WriteLine("Redirect to "+location);
                
                string url = Browser.construct(_host, location);
                status = _m.get(url); // should be ok, some error otherwise
                if (status == 200)
                    _loggedIn = true;
                else
                {
                    Console.WriteLine("Request to "+url+" returned "+status);
                    Console.WriteLine("Therefore not logged in.");
                }
            }
            return _loggedIn;
        }


        public bool getView(View view)
        {
            Console.WriteLine("Change view to {0}", view);

            return true;
        }

        public void init_scan()
        {
            
        }
    }
}
