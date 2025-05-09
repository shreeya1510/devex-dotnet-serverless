using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using IBM.XMS;
using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using devex;

namespace devex
{
    public class create_message_trigger
    {
        private readonly ILogger<create_message_trigger> _logger;
        private static Random random = new Random();

        int randomNumber = random.Next(1, 10);

        public create_message_trigger(ILogger<create_message_trigger> logger)
        {
            _logger = logger;
        }

        [Function("create_message_trigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            // return new OkObjectResult("Add Message");
            string QMGR = req.Query["QMGR"];
            string QUEUE_NAME = req.Query["QUEUE"];
            string MESSAGE = req.Query["MESSAGE"];

            if (string.IsNullOrEmpty(QMGR) || string.IsNullOrEmpty(QUEUE_NAME))
            {
                return new BadRequestObjectResult("QMGR and QUEUE are required");
            }

            MESSAGE ??= "sample message";

            var settings = QueueManagerConfig.GetQmgrSettings(QMGR);

            if (settings == null)
            {
                return new BadRequestObjectResult($"Queue manager '{QMGR}' is not configured.");
            }

            IConnection connectionWMQ;
            IBM.XMS.ISession sessionWMQ;
            IDestination destination;
            IMessageProducer producer;
            ITextMessage textMessage;
            try
            {

                // Get an instance of factory.
                XMSFactoryFactory xff = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);


                // Create WMQ Connection Factory.
                IConnectionFactory cf = xff.CreateConnectionFactory();

                //Set the properties
                // string HOST = GetEnvironmentVariable("HOST");
                // string PORTSTR = GetEnvironmentVariable("PORT");
                // _logger.LogInformation(GetEnvironmentVariable("PORT"));
                // int PORT = int.Parse(PORTSTR);
                // string CHANNEL = GetEnvironmentVariable("CHANNEL");
                // //string QMGR = GetEnvironmentVariable("QMGR");
                // //string QUEUE_NAME = GetEnvironmentVariable("QUEUE_NAME");
                // string APP_USER = GetEnvironmentVariable("APP_USER");
                // string APP_PASSWORD = GetEnvironmentVariable("APP_PASSWORD");
                // string CIPHER_SPEC = GetEnvironmentVariable("CIPHER_SPEC");
                // string KEY_REPOSITORY = GetEnvironmentVariable("KEY_REPOSITORY");
                int PORTT = int.Parse(settings["PORT"]);

                cf.SetStringProperty(XMSC.WMQ_HOST_NAME, settings["HOST"]);
               // _logger.LogInformation(GetEnvironmentVariable("HOST"));
                cf.SetIntProperty(XMSC.WMQ_PORT, PORTT);
                cf.SetStringProperty(XMSC.WMQ_CHANNEL, settings["CHANNEL"]);
               // _logger.LogInformation(GetEnvironmentVariable("CHANNEL"));
                cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
                cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QMGR);
                //_logger.LogInformation(GetEnvironmentVariable("QMGR"));
                cf.SetStringProperty(XMSC.USERID, settings["APP_USER"]);
               // _logger.LogInformation(GetEnvironmentVariable("APP_USER"));
                cf.SetStringProperty(XMSC.PASSWORD, settings["APP_PASSWORD"]);
                //_logger.LogInformation(GetEnvironmentVariable("APP_PASSWORD"));
                cf.SetStringProperty(XMSC.WMQ_SSL_CIPHER_SPEC, settings["CIPHER_SPEC"]);
               // _logger.LogInformation(GetEnvironmentVariable("CIPHER_SPEC"));
                cf.SetStringProperty(XMSC.WMQ_SSL_KEY_REPOSITORY, settings["KEY_REPOSITORY"]);
               // _logger.LogInformation(GetEnvironmentVariable("KEY_REPOSITORY"));

                // Create connection.
                connectionWMQ = cf.CreateConnection();

                // Create session
                sessionWMQ = connectionWMQ.CreateSession(false, AcknowledgeMode.AutoAcknowledge);

                // Create destination
                destination = sessionWMQ.CreateQueue(QUEUE_NAME);

                // Create producer
                producer = sessionWMQ.CreateProducer(destination);

                // Start the connection to receive messages.
                connectionWMQ.Start();

                // Create a text message and send it.
                int quantity = randomNumber;
                for (int i = 0; i < quantity; i++)
                {
                    textMessage = sessionWMQ.CreateTextMessage($"{MESSAGE}: {i + 1}");
                    producer.Send(textMessage);
                }

                // textMessage = sessionWMQ.CreateTextMessage($"Your lucky number is {randomNumber}");

                // producer.Send(textMessage);


                _logger.LogInformation("Message sent to MQ.");

                // Cleanup
                producer.Close();
                destination.Dispose();
                sessionWMQ.Dispose();
                connectionWMQ.Close();
                return new OkObjectResult("Request Accepted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"MQ Exception: {ex.Message}");
                return new OkObjectResult(ex.Message);
            }

        }
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

    }
}
