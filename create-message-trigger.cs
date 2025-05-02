using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using IBM.XMS;
using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Security.Cryptography;

namespace devex
{
    public class create_message_trigger
    {
        private readonly ILogger<create_message_trigger> _logger;
        private static Random random = new Random();

        public string HOST = "localhost";
            public string QMGR = "QM1";
            public int PORT = 1414;
            public string CHANNEL = "DEV.APP.SVRCONN";
            public string QUEUE_NAME = "DEV.QUEUE.1";
            public string APP_USER = "app";
            public string APP_PASSW0RD = "passw0rd";
            public string CIPHER_SPEC = "TLS_RSA_WITH_AES_128_CBC_SHA256";
            public string KEY_REPOSITORY = "*SYSTEM";

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
             try{

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
            

            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, HOST);
            cf.SetIntProperty(XMSC.WMQ_PORT, PORT);
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, CHANNEL);
            cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, QMGR);
            cf.SetStringProperty(XMSC.USERID, APP_USER);
            cf.SetStringProperty(XMSC.PASSWORD, APP_PASSW0RD);
            cf.SetStringProperty(XMSC.WMQ_SSL_CIPHER_SPEC, CIPHER_SPEC);
            cf.SetStringProperty(XMSC.WMQ_SSL_KEY_REPOSITORY, KEY_REPOSITORY);
            // Console.WriteLine("Connection Cipher is set to {0}", "");
            // Console.WriteLine("Key Repository is set to {0}", "");


            // Create connection.
            connectionWMQ = cf.CreateConnection();

            // Create session
            sessionWMQ = connectionWMQ.CreateSession(false, AcknowledgeMode.AutoAcknowledge);

            // Create destination
            destination = sessionWMQ.CreateQueue("DEV.QUEUE.1");

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
            return new OkObjectResult("Message sent to MQ.");
             }
             catch(Exception ex){
                 _logger.LogError($"MQ Exception: {ex.Message}");
                return new OkObjectResult(ex.Message);
             }

        }
    }
}
