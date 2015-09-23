using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Prerender.io.mvc6.Extensions;

namespace Prerender.io.mvc6
{
	public class WebRequestHelper
	{
		private static readonly string _Escaped_Fragment = "_escaped_fragment_";

		private readonly ILogger logger;
		private PrerenderConfiguration options;

		public WebRequestHelper(ILogger logger, PrerenderConfiguration options)
		{
			this.logger = logger;
			this.options = options;
		}

		/// <summary>
		/// Returns false if the request is not eligible for prerender service call
		/// </summary>
		/// <param name="context"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> HandlePrerender(HttpContext context)
		{
			logger.LogVerbose("Checking if request is eligible for prerender service call");
			if (ShouldShowPrerenderedPage(context.Request))
			{
				ResponseResult response = GetPrerenderedPageResponse(context.Request);

				context.Response.StatusCode = (int) response.StatusCode;

				// The WebHeaderCollection is horrible, so we enumerate like this!
				// We are adding the received headers from the prerender service
				for (var i = 0; i < response.Headers.Count; ++i)
				{
					var header = response.Headers.GetKey(i);
					var values = response.Headers.GetValues(i);

					if (values == null) continue;
					context.Response.Headers.Add(header, values);

				}
				await context.Response.WriteAsync(response.ResponseBody);

				return true;
			}

			return false;
		}

		private bool ShouldShowPrerenderedPage(HttpRequest request)
		{
			var userAgent = request.GetUserAgent();

#warning Not sure about this implemetation
			var referer = request.Headers.ContainsKey("Referer") ? request.Headers["Referer"] : string.Empty;

			var url = request.GetFullUrl();

#warning Not sure what this is supposed to do
			if (HasEscapedFragment(request))
			{
				return true;
			}


			if (userAgent.IsEmptyString())
			{
				return false;
			}

			if (!IsSearchRobot(userAgent))
			{
				return false;
			}

			if (IsInResources(url))
			{
				return false;
			}

			var whiteList = options.Whitelist;
			if (whiteList != null && !IsInWhiteList(url, whiteList))
			{
				return false;
			}

			var blacklist = options.Blacklist;
			if (blacklist != null && IsInBlackList(url, referer, blacklist))
			{
				return false;
			}

			return true;

		}

		private bool HasEscapedFragment(HttpRequest request)
		{
			return request.Query.ContainsKey(_Escaped_Fragment);
		}

		#region Search robots handling

		private IEnumerable<string> GetCrawlerUserAgents()
		{
			var crawlerUserAgents = new List<string>(new[]
			{
				"bingbot", "baiduspider", "facebookexternalhit", "twitterbot", "yandex", "rogerbot",
				"linkedinbot", "embedly", "bufferbot", "quora link preview", "showyoubot", "outbrain"
			});

			if (options.CrawlerUserAgents.Count > 0)
			{
				crawlerUserAgents.AddRange(options.CrawlerUserAgents);
			}
			return crawlerUserAgents;
		}

		private bool IsSearchRobot(string userAgent)
		{
			var crawlerUserAgents = GetCrawlerUserAgents();

			// We need to see if the user agent actually contains any of the partial user agents we have!
			// THE ORIGINAL version compared for an exact match...!
			return
				(crawlerUserAgents.Any(
					crawlerUserAgent =>
						userAgent.IndexOf(crawlerUserAgent, StringComparison.InvariantCultureIgnoreCase) >= 0));
		}

		#endregion

		#region Content exclusions handling e.g. docx, zip and similar files

		private IEnumerable<string> GetExtensionsToIgnore()
		{
			var extensionsToIgnore = new List<string>(new[]
			{
				".js", ".css", ".less", ".png", ".jpg", ".jpeg",
				".gif", ".pdf", ".doc", ".txt", ".zip", ".mp3", ".rar", ".exe", ".wmv", ".doc", ".avi", ".ppt", ".mpg",
				".mpeg", ".tif", ".wav", ".mov", ".psd", ".ai", ".xls", ".mp4", ".m4a", ".swf", ".dat", ".dmg",
				".iso", ".flv", ".m4v", ".torrent"
			});

			if (options.ExtensionsToIgnore.Count > 0)
			{
				extensionsToIgnore.AddRange(options.ExtensionsToIgnore);
			}

			return extensionsToIgnore;
		}

		private bool IsInResources(string url)
		{
			var extensionsToIgnore = GetExtensionsToIgnore();
			return extensionsToIgnore.Any(item => url.ToLower().Contains(item.ToLower()));
		}

		#endregion

		#region Whitelist/Blacklist handling

		private bool IsInBlackList(string url, string referer, IEnumerable<string> blacklist)
		{
			return blacklist.Any(item =>
			{
				var regex = new Regex(item);
				return regex.IsMatch(url) || (referer.IsNotEmptyString() && regex.IsMatch(referer));
			});
		}

		private bool IsInWhiteList(string url, IEnumerable<string> whiteList)
		{
			return whiteList.Any(item => new Regex(item).IsMatch(url));
		}

		#endregion


		private string GetApiUrl(HttpRequest request)
		{
			var url = request.GetFullUrl();

			// Correct for HTTPS if that is what the request arrived at the load balancer as 
			// (AWS and some other load balancers hide the HTTPS from us as we terminate SSL at the load balancer!)
			if (request.Headers.ContainsKey("X-Forwarded-Proto") && string.Equals(request.Headers["X-Forwarded-Proto"], "https", StringComparison.InvariantCultureIgnoreCase))
			{
				url = url.Replace("http", "https");
			}
#warning Not sure what this was supposed to do
			//					// Remove the application from the URL
			//					if (options.StripApplicationNameFromRequestUrl && !string.IsNullOrEmpty(request.ApplicationPath) && request.ApplicationPath != "/")
			//					{
			//						// http://test.com/MyApp/?_escape_=/somewhere
			//						url = url.Replace(request.ApplicationPath, string.Empty);
			//					}

			var prerenderServiceUrl = options.PrerenderServiceUrl;
			return prerenderServiceUrl.EndsWith("/")
					? (prerenderServiceUrl + url)
					: "{0}/{1}".Fill(prerenderServiceUrl, url);
		}

		private ResponseResult GetPrerenderedPageResponse(HttpRequest request)
		{
			var apiUrl = GetApiUrl(request);
			var webRequest = (HttpWebRequest)WebRequest.Create(apiUrl);
			webRequest.Method = "GET";
			webRequest.UserAgent = request.GetUserAgent();

			SetProxy(webRequest);
			SetNoCache(webRequest);

			// Add our key!
			if (options.Token.IsNotEmptyString())
			{
				webRequest.Headers.Add("X-Prerender-Token", options.Token);
			}

			try
			{
				// Get the web response and read content etc. if successful
				var webResponse = (HttpWebResponse)webRequest.GetResponse();

				using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
				{
					return new ResponseResult(webResponse.StatusCode, reader.ReadToEnd(), webResponse.Headers);
				}
			}
			catch (WebException e)
			{
				// Handle response WebExceptions for invalid renders (404s, 504s etc.) - but we still want the content
				using (var reader = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8))
				{
					return new ResponseResult(((HttpWebResponse)e.Response).StatusCode, reader.ReadToEnd(), e.Response.Headers);
				}
			}
		}

		private static void SetNoCache(HttpWebRequest webRequest)
		{
			webRequest.Headers.Add("Cache-Control", "no-cache");
			webRequest.ContentType = "text/html";
		}

		private void SetProxy(HttpWebRequest webRequest)
		{
			if (options.Proxy.IsNotNull() && options.Proxy.Url.IsNotEmptyString())
			{
				webRequest.Proxy = new WebProxy(options.Proxy.Url, options.Proxy.Port);
			}
		}
	}
}