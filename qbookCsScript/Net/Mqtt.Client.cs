
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using OpenCvSharp.Aruco;
using QB.Automation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QB.Net
{
    public partial class Mqtt
    {
        public class Client : Machine
        {
            public class MqttServerMessageReceivedEventArgs : EventArgs
            {
                //    public char Dcb = ' ';
                //    public int Port = 0;
                public string Topic = "";
                public string Value = "";
                //     public string Channel = "K0";
                //     public string[] Parameters = null;
            }

            public delegate void OnMessageReceivedDelegate(Client client, MqttServerMessageReceivedEventArgs ea);// int port =0, string client="", char dcb=' ', string command="????", string channel="K0", string[] parameters = null);
            public OnMessageReceivedDelegate OnMessageReceived;

            // !!! JUST one MQTT client per qbook
            static  MqttFactory mqttFactory = new MqttFactory();
            static Dictionary<string, IManagedMqttClient> managedMqttClients = new Dictionary<string, IManagedMqttClient>();


            public override void Destroy()
            {
                // base.Destroy();

                try
                {

                }
                catch { }


                this.OnMessageReceived = null;  //removes all existing subscribers                
            }

            string Uri;

            public Client(string name, string uri = "127.0.0.1:1883", string user = "", string password = "") : base(name)
            {
                Uri = uri;
               // Destroy();
                //    using (var managedMqttClient = mqttFactory.CreateManagedMqttClient())

                if (managedMqttClients.ContainsKey(uri))
                {
                    managedMqttClients[uri].ApplicationMessageReceivedAsync -= ManagedMqttClient_ApplicationMessageReceivedAsync;
                    managedMqttClients[uri].Dispose();
                }
                else
                {
                    managedMqttClients.Add(uri, mqttFactory.CreateManagedMqttClient());
                }


                managedMqttClients[uri] = mqttFactory.CreateManagedMqttClient();
                {
                    if (password == null)
                    {
                        var mqttClientOptions = new MqttClientOptionsBuilder()
                                                .WithTcpServer(uri.Split(':')[0], int.Parse(uri.Split(':')[1]))

                                                .Build();

                        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                            .WithClientOptions(mqttClientOptions)
                            .Build();

                        //await 
                        managedMqttClients[uri].StartAsync(managedMqttClientOptions);
                    }
                    else
                    {
                        var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(uri.Split(':')[0], int.Parse(uri.Split(':')[1]))
                        .WithCredentials(user, password)
                        //      .WithTls()
                        .Build();

                        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                            .WithClientOptions(mqttClientOptions)
                            .Build();

                        //await 
                        managedMqttClients[uri].StartAsync(managedMqttClientOptions);
                    }



                }
                managedMqttClients[uri].ApplicationMessageReceivedAsync += ManagedMqttClient_ApplicationMessageReceivedAsync; //+= delegate (MqttApplicationMessageReceivedEventArgs args)
            }

            public void Publish(string topic, string payload, bool retain = false)
            {
                // The application message is not sent. It is stored in an internal queue and
                // will be sent when the client is connected.
                // await 
                if (managedMqttClients[Uri].IsConnected)

                    managedMqttClients[Uri].EnqueueAsync(topic, payload, retain: retain);

                // Wait until the queue is fully processed.
                //      SpinWait.SpinUntil(() => managedMqttClient.PendingApplicationMessagesCount == 0, 10000);

                Console.WriteLine($"Pending messages = {managedMqttClients[Uri].PendingApplicationMessagesCount}");
                //{
                // Do some work with the message...

                // Now respond to the broker with a reason code other than success.
                //    args.ReasonCode = MqttApplicationMessageReceivedReasonCode.ImplementationSpecificError;
                //    args.ResponseReasonString = "That did not work!";

                // User properties require MQTT v5!
                //**    args.ResponseUserProperties.Add(new MqttUserProperty("My", "Data"));

                // Now the broker will resend the message again.
                //  return Task.CompletedTask;
                // };
            }

            private Task ManagedMqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
            {
                string name = arg.ApplicationMessage.Topic;
                if (arg.ApplicationMessage.Payload != null)
                {
                    string text = System.Text.Encoding.UTF8.GetString(arg.ApplicationMessage.Payload, 0, arg.ApplicationMessage.Payload.Length);
                    Console.WriteLine(arg.ApplicationMessage.Topic + ":" + text);
                    //  return Task.CompletedTask;


                    double value;
                    double.TryParse(text, out value);

                    /*
                    if (!Signals.ContainsKey(name))
                        Signals.TryAdd(name, new Signal(name));
                    */

                    if (OnMessageReceived != null)
                    {
                        MqttServerMessageReceivedEventArgs ea = new MqttServerMessageReceivedEventArgs();
                        ea.Topic = name;
                        ea.Value = text;
                        OnMessageReceived(this, ea);// port, client, dcb, command, channel, parameter.Trim().Split());
                    }


                    Signals[name].Value = value;
                    Signals[name].lastValueUpdate = DateTime.Now;
                }

                return Task.CompletedTask;
            }

            public void Subscribe(string topic)
            {
                /*
                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                    f.WithTopic(topic);// "mqttnet/samples/topic/2");
                    })
                .Build();
                */
                //await 
                //  managedMqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                managedMqttClients[Uri].SubscribeAsync(topic);
            }

            public void Unsubscribe(string topic)
            {
                /*
                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                    f.WithTopic(topic);// "mqttnet/samples/topic/2");
                    })
                .Build();
                */
                //await 
                //  managedMqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                managedMqttClients[Uri].UnsubscribeAsync(topic);
            }


       //     public static Task UnsubscribeAsync(this IManagedMqttClient managedMqttClient, string topic)

           
        }
    }
}