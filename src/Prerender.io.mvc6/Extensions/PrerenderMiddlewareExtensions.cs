using Microsoft.AspNet.Builder;

namespace Prerender.io.mvc6.Extensions
{
	public static class PrerenderMiddlewareExtensions
	{
		public static IApplicationBuilder UsePrerender(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<PrerenderMiddleware>();
		}
	}
}