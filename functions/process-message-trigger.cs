using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using IBM.XMS;

namespace devex
{
    public class process_message_trigger
    {
        private readonly ILogger<process_message_trigger> _logger;

        // private const int TIMEOUTTIME = 30000;
        public process_message_trigger(ILogger<process_message_trigger> logger)
        {
            _logger = logger;
        }

        [Function("process_message_trigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var logger = context.GetLogger("process_message_trigger");
            logger.LogInformation("Trigger received. Starting background MQ work...");

            // Fire-and-forget
            // _ = Task.Run(() => HandleMqWorkAsync(logger));
            await HandleMqWorkAsync(_logger); 
            // Return immediately to avoid proxy timeouts
            return new AcceptedResult(); // HTTP 202
        }
        private async Task HandleMqWorkAsync(ILogger logger)
        {
            try
            {
                IConnection connectionWMQ;
                IBM.XMS.ISession sessionWMQ;
                IDestination destination;
                IMessageConsumer consumer;
                ITextMessage textMessage;

                // Get an instance of factory.
                XMSFactoryFactory xff = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);

                // Create WMQ Connection Factory.
                IConnectionFactory cf = xff.CreateConnectionFactory();

                // Set the properties
                // cf.SetStringProperty(XMSC.WMQ_HOST_NAME, "qmhe-5273.qm.us-south.mq.appdomain.cloud");
                // cf.SetIntProperty(XMSC.WMQ_PORT, 32316);
                // cf.SetStringProperty(XMSC.WMQ_CHANNEL, "CLOUD.APP.SVRCONN");
                // cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
                // cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, "QMHE");
                // cf.SetStringProperty(XMSC.USERID, "jakartatest");
                // cf.SetStringProperty(XMSC.PASSWORD, "QzuQLvV_j7LZyVDMcQ9O5geJltt6--gG4_eS4wcVu6rd");
                // cf.SetStringProperty(XMSC.WMQ_SSL_CIPHER_SPEC, "TLS_RSA_WITH_AES_128_CBC_SHA256");
                // cf.SetStringProperty(XMSC.WMQ_SSL_KEY_REPOSITORY, "*SYSTEM");

                string HOST = GetEnvironmentVariable("HOST");
                string PORTSTR = GetEnvironmentVariable("PORT");
                 _logger.LogInformation(GetEnvironmentVariable("PORT"));
                int PORT = int.Parse(PORTSTR);
                string CHANNEL = GetEnvironmentVariable("CHANNEL");
                string QMGR = GetEnvironmentVariable("QMGR");
                string QUEUE_NAME = GetEnvironmentVariable("QUEUE_NAME");
                string APP_USER = GetEnvironmentVariable("APP_USER");
                string APP_PASSWORD = GetEnvironmentVariable("APP_PASSWORD");
                string CIPHER_SPEC = GetEnvironmentVariable("CIPHER_SPEC");
                string KEY_REPOSITORY = GetEnvironmentVariable("KEY_REPOSITORY");

                cf.SetStringProperty(XMSC.WMQ_HOST_NAME, HOST);
                _logger.LogInformation(GetEnvironmentVariable("HOST"));
                cf.SetIntProperty(XMSC.WMQ_PORT, PORT);
                cf.SetStringProperty(XMSC.WMQ_CHANNEL, CHANNEL);
                 _logger.LogInformation(GetEnvironmentVariable("CHANNEL"));
                cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
                cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QMGR);
                 _logger.LogInformation(GetEnvironmentVariable("QMGR"));
                cf.SetStringProperty(XMSC.USERID, APP_USER);
                 _logger.LogInformation(GetEnvironmentVariable("APP_USER"));
                cf.SetStringProperty(XMSC.PASSWORD, APP_PASSWORD);
                 _logger.LogInformation(GetEnvironmentVariable("APP_PASSWORD"));
                cf.SetStringProperty(XMSC.WMQ_SSL_CIPHER_SPEC, CIPHER_SPEC);
                 _logger.LogInformation(GetEnvironmentVariable("CIPHER_SPEC"));
                cf.SetStringProperty(XMSC.WMQ_SSL_KEY_REPOSITORY, KEY_REPOSITORY);
                 _logger.LogInformation(GetEnvironmentVariable("KEY_REPOSITORY"));

                 // Create connection.
                connectionWMQ = cf.CreateConnection();

                // Create session
                sessionWMQ = connectionWMQ.CreateSession(false, AcknowledgeMode.AutoAcknowledge);

                // Create destination
                destination = sessionWMQ.CreateQueue(QUEUE_NAME);

                // Create consumer
                consumer = sessionWMQ.CreateConsumer(destination);

                // Start the connection to receive messages.
                connectionWMQ.Start();

                textMessage = (ITextMessage)consumer.Receive();
                await Task.Delay(10000);

                if (textMessage.Text == null)
                {
                    _logger.LogInformation("No MQ Message Received");
                }

                _logger.LogInformation(textMessage.Text);

                var result = new OkObjectResult("Message Recieved");

                consumer.Close();
                destination.Dispose();
                sessionWMQ.Dispose();
                connectionWMQ.Close();
            }
            catch (XMSException ex)
            {
                _logger.LogError($"XMS Exception caught: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"MQ Exception caught: {ex.Message}");
            }
        }
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

    }
}

