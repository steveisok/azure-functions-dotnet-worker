using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HelloHttp
{
    public sealed class HelloHttp
    {
        private readonly ILogger _logger;

        public HelloHttp(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HelloHttp>();
        }

        [Function("HelloHttp")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var buildConfiguration = string.Empty;
#if DEBUG
            buildConfiguration = "Debug";
#else
        buildConfiguration = "Release";
#endif

            _logger.LogInformation($"C# HTTP trigger function processed a request.buildConfiguration:{buildConfiguration}");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.WriteString($"Hello World!");
            return response;
        }
    }
}
