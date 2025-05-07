using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using IBM.XMS;
using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace devex
{
    public class create_message_trigger
    {
        private readonly ILogger<create_message_trigger> _logger;
        private static Random random = new Random();

        int randomNumber = random.Next(1, 1000);

        public create_message_trigger(ILogger<create_message_trigger> logger)
        {
            _logger = logger;
        }

        [Function("create_message_trigger")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            

            // return new OkObjectResult("Add Message");

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

                // Create producer
                producer = sessionWMQ.CreateProducer(destination);

                // Start the connection to receive messages.
                connectionWMQ.Start();

                // Create a text message and send it.
                textMessage = sessionWMQ.CreateTextMessage($"Your lucky number is {randomNumber}");

                producer.Send(textMessage);


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
