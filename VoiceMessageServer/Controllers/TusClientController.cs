using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TusDotNetClient;
using VoiceMessageServer.Extensions;

namespace VoiceMessageServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TusClientController : ControllerBase
    {
        private readonly ILogger<TusClientController> _logger;

        public TusClientController(ILogger<TusClientController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<bool> upload()
        {
            var file = new FileInfo("appsettings.json");
            var client = new TusClient();
            string TusServerEndpoint = $"{VoiceMessageServerHttpContext.AppBaseUrl}/files";

            var metadata = new (string key, string value)[] {
                (key: "fileNameWithExtension", value: "appsettings.json"),
                (key: "randomKey", value: "randomValue"),
            };

            var fileUrl = await client.CreateAsync(
                TusServerEndpoint, file.Length, metadata);

            var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

            uploadOperation.Progressed += (transferred, total) =>
                _logger.LogDebug($"Progress: {transferred}/{total}");

            await uploadOperation;

            return true;
        }
    }
}
