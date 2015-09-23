using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Prerender.io.mvc6
{
	public class RequestLoggerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IOptions<PrerenderConfiguration> configuration;
		private readonly ILogger logger;

		public RequestLoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<PrerenderConfiguration> configuration)
		{
			_next = next;
			this.configuration = configuration;
			logger = loggerFactory.CreateLogger<RequestLoggerMiddleware>();
		}

		public async Task Invoke(HttpContext context)
		{
			logger.LogInformation(configuration.Options.PrerenderServiceUrl);
			logger.LogInformation("Handling request: " + context.Request.Path);
			await _next.Invoke(context);
			logger.LogInformation("Finished handling request.");
		}

	}
}