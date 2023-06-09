﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace VoiceMessageServer.Extensions
{
    public class VoiceMessageServerHttpContext
    {
        private static IHttpContextAccessor m_httpContextAccessor;

        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            m_httpContextAccessor = contextAccessor;
        }
    }

    public static class HttpContextExtensions
    {
        public static void AddVoiceMessageServerHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            VoiceMessageServerHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}
