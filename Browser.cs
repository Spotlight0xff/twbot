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
        private string _useragent;

        public Browser()
        {
            _cookieJar = new CookieContainer();
            _content = null;
            _status = 0;
            _url = null;
            _useragent = "TWBot Dev";
            _redirect = null;
        }

        /*
         * string url: URL to send GET-Request
         * returns the http status code
         */
        public int get(string url)
        {
            try
            { 
                // transform the given url to an uri
                _url = new Uri(url);
            } catch (Exception e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return 0;
            }
            return get(_url);
        }

        // see get(string url)
        public int get(Uri uri)
        {

            // display URI
            Console.WriteLine("GET "+uri);

            HttpWebRequest req = null;
            try
            {
                
                req = (HttpWebRequest) WebRequest.Create(uri);   
            } catch (Exception e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return 0;
            }

            // use global cookie container
            req.CookieContainer = _cookieJar;

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            } catch ( WebException e)
            {
                Console.WriteLine("browser::get() exception: {0}", e.Message);
                return 0;
            }

            // retrieve response and save in _content
            Stream response_stream = response.GetResponseStream();
            StreamReader response_streamer = new StreamReader(response_stream);
            _content = response_streamer.ReadToEnd();

            response_stream.Close();
            response_streamer.Close();
            response.Close();

            _status = (int)response.StatusCode;
            Console.WriteLine("==> "+_status);
            return _status;

        }

        /*
         * Requests the host with a POST-method query using data as parameters
         * returns the http status code
         */
        public int post(string url, string data)
        {
            try
            {
                _url = new Uri(url);

            }
            catch (Exception e)
            {
                Console.WriteLine("browser::post() exception: {0}", e.ToString());
                return 0;
            }
            return post(_url, data);
        }

        public int post(Uri uri, string data)
        {

            Console.WriteLine("POST " + uri);
            Console.WriteLine("data: '" + data + "'");
            CookieContainer cookieJar_tmp = new CookieContainer();
            HttpWebRequest req = null;
            try
            {

                req = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine("browser::post() exception: {0}", e.ToString());
                return 0;
            }

            // change to POST
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.UserAgent = _useragent;
            req.CookieContainer = cookieJar_tmp;
            req.AllowAutoRedirect = false;

            // encode our post data using ascii
            byte[] data_bytes = Encoding.ASCII.GetBytes(data);

            // write post data to request stream
            try
            {
                using (Stream post_data = req.GetRequestStream())
                {
                    post_data.Write(data_bytes, 0, data.Length);
                    post_data.Close();
                }
            } catch (WebException e)
            {
                Console.WriteLine("browser::post() exception: " + e.Message);
                return 0;
            }
            
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("browser::post() exception: " + e.Message);
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
            }


            
            
            

            // get response stream and write it to _content.
            Stream response_stream = null;
            try
            {
                response_stream = response.GetResponseStream();
            } catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            StreamReader response_streamer = new StreamReader(response_stream);
            _content = response_streamer.ReadToEnd();
            response_streamer.Close();
            response_stream.Close();
            response.Close();

            Console.WriteLine("==> "+_status);
            return _status;
        }

        /*
         * Refreshes the current page, only GET-requests (POST will not be refreshed)
         * returns http status code (just like get() and post()) 
         */
        public int refresh()
        {
            // just call get on the current url for now.
            return get(_url);
        }

        public string getContent()
        {
            return _content;
        }

        public string getUrl()
        {
            return _url.ToString();
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
