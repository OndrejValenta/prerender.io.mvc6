using Microsoft.AspNet.Http;

namespace Prerender.io.mvc6.Extensions
{
	public static class HttpRequestExtensions
	{
		public static string GetFullUrl(this HttpRequest request)
		{
			if (request.Path == "/")
				return "{0}://{1}".Fill(request.Scheme, request.Host.Value);

			return "{0}://{1}{2}".Fill(request.Scheme, request.Host, request.Path);
		}

		public static string GetUserAgent(this HttpRequest request)
		{
			return request.Headers.ContainsKey("User-Agent") ? request.Headers["User-Agent"] : string.Empty;
		}
	}
}