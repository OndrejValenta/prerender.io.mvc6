using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Abstractions;

namespace Prerender.io.mvc6
{
	using Microsoft.AspNet.Builder;

	public static class RequestLoggerExtensions
	{
		public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RequestLoggerMiddleware>();
		}
	}

}
