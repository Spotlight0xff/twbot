using System;
using System.Net;
using System.Threading;


namespace twbot
{
    class Research : Module
    {
        
 
        // should be started as a thread
        // handles the research in every village
        // use pause_research() to pause researching and continue_research() to continue
        public override void doWork()
        {
            string research = "";
            bool queue = false;
            string content;
            _active = true;
            while (_active)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.id;
                    _browser.get(Parse.viewUrl(_host, id, "smith")); // get the overview to get research levels
                    content = _browser.getContent();
                    
                    // dirty method: search for links to research
                    string path = Parse.searchLink(content, "action=research&");
                    if (path == null)
                    { // no link found
                        continue;
                    }
                    research = Parse.retrieveParam(path, "id");
                    Console.WriteLine("[research:{0}] do  "+research, id);
                    string url = Browser.construct(_host, path);
                    int status = _browser.get(url);
                    if (status != 302)
                    { // some error must've occured
                        if (_debug)
                        {
                            Console.WriteLine("[research:{0}] Could not research {1} (see id_{0}_research_error.html)", id, research);
                            _browser.save("id_"+id+"_research_error.html");
                        }else
                        {
                            Console.WriteLine("[research:{0}] could not research, probably not enough resources");
                        }
                    }
                    
                    //Thread.Sleep(_researchspeed); // not implemented yet!
                }
            }
        }

       // pauses the researching process
        public override void pauseWork()
        {
            _active = false;
        }

        // continues the researching process
        public override void continueWork()
        {
            _active = true;
        }
    }
}
