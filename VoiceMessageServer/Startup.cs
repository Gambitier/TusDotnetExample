using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;
using VoiceMessageServer.Extensions;

namespace VoiceMessageServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly string TusFileStoreBaseDir = "voiceMessageFiles";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Directory.CreateDirectory(TusFileStoreBaseDir);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddVoiceMessageServerHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseHttpContext();

            app.UseAuthorization();

            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                // This method is called on each request so different configurations can be returned per user, domain, path etc.
                // Return null to disable tusdotnet for the current request.

                Store = new TusDiskStore(TusFileStoreBaseDir),
                // On what url should we listen for uploads?
                UrlPath = "/files",
                Events = new Events
                {
                    OnFileCompleteAsync = async eventContext =>
                    {
                        ITusFile file = await eventContext.GetFileAsync();
                        Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
                        using Stream content = await file.GetContentAsync(eventContext.CancellationToken);
                        {
                            await SaveFile(content, metadata);
                        }
                    }
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task<bool> SaveFile(Stream content, Dictionary<string, Metadata> metadata)
        {
            metadata.TryGetValue("fileNameWithExtension", out Metadata fileNameWithExtensionFromMetadata);
            var fileNameWithExtension = !fileNameWithExtensionFromMetadata.HasEmptyValue
                ? fileNameWithExtensionFromMetadata.GetString(System.Text.Encoding.UTF8)
                : Guid.NewGuid().ToString();

            var basePath = Path.Join(TusFileStoreBaseDir, Guid.NewGuid().ToString());
            Directory.CreateDirectory(basePath);

            var filePath = Path.Join(basePath, fileNameWithExtension);
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await content.CopyToAsync(fileStream);
            }

            return true;
        }
    }
}
