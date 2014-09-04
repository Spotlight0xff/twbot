using System;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Text;


namespace twbot
{
    class Browser
    {
        private Uri _url;
        private string _content;
        private int _status;
        private CookieContainer _cookieJar;
        private string _redirect;

        public Browser()
        {
            _cookieJar = new CookieContainer();
            _content = null;
            _status = 0;
            _url = null;
        }


        public bool get(string url)
        {
            try
            { 
                // transform the given url to an uri
                _url = new Uri(url);
            } catch (Exception e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return false;
            }
            return get(_url);
        }

        public bool get(Uri uri)
        {
            Console.WriteLine("Request "+uri);
            HttpWebRequest req = null;
            try
            {
                
                req = (HttpWebRequest) WebRequest.Create(uri);   
            } catch (Exception e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return false;
            }
            Console.WriteLine(_cookieJar.Count);
            Console.WriteLine("Cookie count: " + _cookieJar.Count);
           /* foreach (Cookie cookie in _cookieJar)
            {
                Console.WriteLine(cookie.ToString());
            }*/
            req.CookieContainer = _cookieJar;

            Console.WriteLine(req.Headers.ToString()); // DEBUG
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            } catch ( WebException e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return false;
            }

            Console.WriteLine("Status: {0}", (int) response.StatusCode);

            Stream response_stream = response.GetResponseStream();
            StreamReader response_streamr = new StreamReader(response_stream);
            _content = response_streamr.ReadToEnd();
            Console.WriteLine(_content);

            return true;

        }

        /*
         * Requests the host with a POST-method query using data as parameters
         * returns true for success, false otherwise
         */
        public int post(string url, string data)
        {
            try
            {
                _url = new Uri(url);

            }
            catch (Exception e)
            {
                Console.WriteLine("browser::post() exception: {0}", e.Message);
                return 0;
            }
            return post(_url, data);
        }

        public int post(Uri uri, string data)
        {
            CookieContainer cookieJar_tmp = new CookieContainer();
            HttpWebRequest req = null;
            try
            {

                req = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return 0;
            }
            Console.WriteLine(uri);

            // change to POST
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.UserAgent = "TWBot";
            req.Referer = "http://192.168.2.100/index.php";
            req.CookieContainer = cookieJar_tmp;
            req.KeepAlive = true;
            req.AllowAutoRedirect = false;

            // encode our post data using ascii
            byte[] data_bytes = Encoding.ASCII.GetBytes(data);

            // write post data to request stream
            Stream post_data = req.GetRequestStream();
            post_data.Write(data_bytes, 0, data.Length);
            post_data.Close();
            //Console.WriteLine(req.Headers.ToString()); // DEBUG

            
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return 0;
            }

            // save statuscode & redirection-urk
            _status = (int)response.StatusCode;
            _redirect = response.Headers["Location"];

            // add cookie to global cookiecontainer (we need to modify it to be global)
            foreach ( Cookie cookie in cookieJar_tmp.GetCookies(_url))
            {
                cookie.Path = "/"; // change Path to global
                _cookieJar.Add(cookie);
                Console.WriteLine(_url + " | " + cookie.ToString() + " @ "+ cookie.Path);
            }


            
            
            

            // get response stream and write it to _content.
            Stream response_stream = response.GetResponseStream();
            StreamReader response_streamer = new StreamReader(response_stream);
            _content = response_streamer.ReadToEnd();
            response_streamer.Close();
            response_stream.Close();
            response.Close();
            Console.WriteLine(_content); // DEBUG

            return _status;
        }

        /*
         * Refreshes the current page, only GET-requests (POST will not be refreshed)
         * returns true for success, false for error
         */
        public bool refresh()
        {
            // just call get on the current url for now.
            return get(_url);
        }

        public string getContent()
        {
            return _content;
        }

        public string getRedirect()
        {
            return _redirect;
        }

        public static string construct(string host, string path = null, string query = null, string fragment = null)
        {

            // build "http://host/path?query#fragment"
            StringBuilder str_build = new StringBuilder("http://");
            str_build.Append(host);
            if (path != null)
            {
                str_build.AppendFormat("/{0}", path);
                if (query != null)
                {
                    str_build.AppendFormat("?{0}", query);
                    if (fragment != null)
                    {
                        str_build.AppendFormat("#{0}", fragment);
                    }
                }
            }
            return str_build.ToString();
        }
    }
}
