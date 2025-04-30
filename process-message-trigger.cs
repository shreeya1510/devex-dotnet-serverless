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
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

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

            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, "localhost");
            cf.SetIntProperty(XMSC.WMQ_PORT, 1414);
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, "DEV.APP.SVRCONN");
            cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, "QM1");
            cf.SetStringProperty(XMSC.USERID, "app");
            cf.SetStringProperty(XMSC.PASSWORD, "passw0rd");


             IConnection connectionWMQ;
             IBM.XMS.ISession sessionWMQ;
             IDestination destination;
             IMessageConsumer consumer;
             ITextMessage textMessage;

             connectionWMQ = cf.CreateConnection();

            // Create session
            sessionWMQ = connectionWMQ.CreateSession(false, AcknowledgeMode.AutoAcknowledge);

            // Create destination
            destination = sessionWMQ.CreateQueue("DEV.QUEUE.1");

            // Create consumer
            consumer = sessionWMQ.CreateConsumer(destination);

            // Start the connection to receive messages.
            connectionWMQ.Start();

            textMessage = (ITextMessage)consumer.Receive();

            consumer.Close();
            destination.Dispose();
            sessionWMQ.Dispose();
            connectionWMQ.Close();

            return new OkObjectResult("Message Recieved");
        }
    }
}
