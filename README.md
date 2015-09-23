### Welcome to Prerender.io.mvc6.
This is my implementation of Prerender.io MVC(https://github.com/greengerong/Prerender_asp_mvc) in new ASP.NET 5.

### State of play
So far I've just converted @greengerong code to new ASP.NET 5 style, it should work but had no time to test it yet. There is still some work on TestWeb and perhaps some performance/memory handling testing will be needed. Also handling of configuration file is somewhat troublesome, because all secrets lay in JSON file. I'll add new layer of Environment/UserSecret configuration.

### Implementation details
* PrerenderMiddleware - Implementation of OWIN middleware
* WebRequestHelper - Where all the magic is going on. Each request is first assessed with several rules to see if Prerender service call is needed, if so WebRequest is created.
* prerender.io.config.json - configuration file with all available configuration values
* PrerenderConfiguration.cs - configuration is loaded into this class with this piece of code `services.AddOptions();
     services.Configure<PrerenderConfiguration>(Configuration.GetSection("PrerenderConfiguration"));`

### Support or Contact
If you have any suggestions or questions you can contact me on [ovalenta@spaneco.com](mailto://ovalenta@spaneco.com).
