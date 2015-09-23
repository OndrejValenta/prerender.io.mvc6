using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Prerender.io.mvc6
{
	public class PrerenderMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IOptions<PrerenderConfiguration> configuration;
		private readonly ILogger logger;
		private WebRequestHelper helper;

		public PrerenderMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<PrerenderConfiguration> configuration)
		{
			_next = next;
			this.configuration = configuration;
			logger = loggerFactory.CreateLogger<RequestLoggerMiddleware>();
			helper = new WebRequestHelper(logger, configuration.Options);
    }

		public async Task Invoke(HttpContext context)
		{
			logger.LogInformation(configuration.Options.PrerenderServiceUrl);
			logger.LogInformation("Handling request: " + context.Request.Path);

			if (await helper.HandlePrerender(context))
			{
				await _next.Invoke(context);
			}

			logger.LogInformation("Finished handling request.");
		}
	}
}