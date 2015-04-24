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
        /// <summary>
        /// Queries a website using HTTP-GET
        /// <seealso cref="get(System.Uri)"/>
        /// </summary>
        /// <param name="url">the URL to the website</param>
        ///	<returns>HTTP status code or 0 on error</returns>
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

        /// <summary>
        /// Queries a website using HTTP-Get
        /// <seealso cref="get(System.String)" />
        /// </summary>
        /// <param name="uri">Uri object to the website</param>
        ///	<returns>HTTP status code or 0 on error</returns>
        public int get(Uri uri)
        {
            // Debug.WriteLine("GET "+uri);

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
            req.AllowAutoRedirect = false;

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
            // Console.WriteLine("==> "+_status);
            return _status;

        }

        /// <summary>
        /// Requests a Website with POST using the specified payload
        /// </summary>
        /// <param name="url">URL to the webpage</param>
        /// <param name="data">POST-Payload</param>
        ///	<returns>HTTP Statuscode or 0 on error</returns>
        public int post(string url, string data)
        {
            try
            {
                _url = new Uri(url);

            }
            catch (Exception e)
            {
                Console.WriteLine("browser::post() exception: {0}", e.ToString());
                throw;
            }
            return post(_url, data);
        }

        // see 'public int post(string url, string data)'
        /// <summary>
        /// Requests a Webpage with POST using a specified payload
        /// </summary>
        /// <param name="uri">Uri object of the webpage</param>
        /// <param name="data">POST payload</param>
        ///	<returns>HTTP Statuscode or 0 on error</returns>
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
                throw;
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
                throw;
            }
            
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("browser::post() exception: " + e.Message);
                throw;
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

        /// <summary>
        /// Sets the cookie container of the Browser session
        /// </summary>
        /// <param name="cj">Cookie container using the new cookies</param>
        public void setCookies(CookieContainer cj)
        {
            _cookieJar = cj;
        }

        /// <summary>
        /// gets the current cookie container
        /// </summary>
        ///	<returns>current cookie container</returns>
        public CookieContainer getCookies()
        {
            return _cookieJar;
        }

        /// <summary>
        /// Refreshes the current page, only per GET
        /// </summary>
        ///	<returns>HTTP Statuscode of the refresh call</returns>
        public int refresh()
        {
            // just call get on the current url for now.
            return get(_url);
        }

        /// <summary>
        /// returns HTML content of the current page without refreshing the page
        /// </summary>
        ///	<returns>HTML content</returns>
        public string getContent()
        {
            return _content;
        }

        /// <summary>
        /// returns current URL
        /// </summary>
        ///	<returns>current URL as string</returns>
        public string getUrl()
        {
            return _url.ToString();
        }

        /// <summary>
        /// returns redirect URL (when the statuscode was 302)
        /// </summary>
        ///	<returns>redirect URL as string</returns>
        public string getRedirect()
        {
            return _redirect;
        }

        /// <summary>
        /// Writes HTML content into a file
        /// </summary>
        /// <param name="file">file to save the content</param>
        public void save(string file)
        {
            if (_content != null)
            {
                System.IO.File.WriteAllText(@file, _content);
            }
        }
            

        // constructs a url with the provided arguments
        /// <summary>
        /// Constructs an URL with the provided arguments
        /// </summary>
        /// <param name="host">Host domain or IP</param>
        /// <param name="path">Path to page</param>
        /// <param name="query">Query argument</param>
        /// <param name="fragment">Query value/fragment</param>
        ///	<returns>constructed URL</returns>
        public static string construct(string host, string path = null, string query = null, string fragment = null)
        {
            // build "http://host/path?query#fragment"
            StringBuilder str_build = new StringBuilder("http://");
            str_build.Append(host);
            if (path != null)
            {
                path = path.Replace("&amp;", "&");
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
