using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace devex
{
    public class create_message_trigger
    {
        private readonly ILogger<create_message_trigger> _logger;

        public create_message_trigger(ILogger<create_message_trigger> logger)
        {
            _logger = logger;
        }

        [Function("create_message_trigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
