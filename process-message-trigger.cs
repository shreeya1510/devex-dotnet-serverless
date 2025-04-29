using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace devex
{
    public class process_message_trigger
    {
        private readonly ILogger<process_message_trigger> _logger;

        public process_message_trigger(ILogger<process_message_trigger> logger)
        {
            _logger = logger;
        }

        [Function("process_message_trigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Process Message");
        }
    }
}
