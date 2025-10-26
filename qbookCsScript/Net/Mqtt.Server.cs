//using CefSharp.DevTools.DOM;
using MQTTnet;
using MQTTnet.Server;
using QB.Automation;
using System;
using System.Text;
using System.Threading.Tasks;



namespace QB.Net
{
    public partial class Mqtt
    {

        public class Server : Machine
        {
            public Server(string name) : base(name)
            {
                Start();
            }

            void Start()
            {
                Console.WriteLine("The managed MQTT server is started.");

                // Create the options for MQTT Broker
                var options = new MqttServerOptionsBuilder()
                    //Set endpoint to localhost
                    .WithDefaultEndpoint();
                // Create a new mqtt server
                var server = new MqttFactory().CreateMqttServer(options.Build());
                //Add Interceptor for logging incoming messages
                server.InterceptingPublishAsync += Server_InterceptingPublishAsync;
                // Start the server
                server.StartAsync();

            }


            Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
            {
                // Convert Payload to string
                var payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);


                return Task.CompletedTask;

                Console.WriteLine(
                    "******************* TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",

                    DateTime.Now,
                    arg.ClientId,
                    arg.ApplicationMessage?.Topic,
                    payload,
                    arg.ApplicationMessage?.QualityOfServiceLevel,
                    arg.ApplicationMessage?.Retain);
                return Task.CompletedTask;
            }
        }
    }
}