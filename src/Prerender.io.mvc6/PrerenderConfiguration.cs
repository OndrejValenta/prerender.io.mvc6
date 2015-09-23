using System.Collections.Generic;

namespace Prerender.io.mvc6
{
	public class PrerenderConfiguration
	{
		public string PrerenderServiceUrl { get; set; } = "http://service.prerender.io/";

		public bool StripApplicationNameFromRequestUrl { get; set; } = false;

		public List<string> Whitelist { get; set; }

		public List<string> Blacklist { get; set; }

		public List<string> ExtensionsToIgnore { get; set; }

		public List<string> CrawlerUserAgents { get; set; }

		public ProxyConfiguration Proxy { get; set; }

		public string Token { get; set; }

		public PrerenderConfiguration()
		{

		}
	}

	public class ProxyConfiguration
	{
		public string Url { get; set; }

		public int Port { get; set; } = 80;
	}
}
