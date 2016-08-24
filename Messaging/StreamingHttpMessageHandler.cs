﻿using System;
using Grapevine.Client;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Packets;

namespace QuantConnect.Messaging
{

    /// <summary>
    /// Message handler that sends messages over http as soon as the messages arrive.
    /// </summary>
    public class StreamingHttpMessageHandler : IMessagingHandler
    {
        private AlgorithmNodePacket _job;

        // Port to send data to
        public static readonly string Port = Config.Get("http-port");

        // Client for sending asynchronous requests.
        private static readonly RESTClient Client = new RESTClient("http://localhost.fiddler:" + Port);

        
        /// <summary>
        /// Gets or sets whether this messaging handler has any current subscribers.
        /// This is not used in this message handler.  Messages are sent via http as they arrive
        /// </summary>
        public bool HasSubscribers { get; set; }

        /// <summary>
        /// Initialize the messaging system
        /// </summary>
        public void Initialize()
        {
            // put the port to start on here
        }

        /// <summary>
        /// Set the user communication channel
        /// </summary>
        /// <param name="job"></param>
        public void SetAuthentication(AlgorithmNodePacket job)
        {
            _job = job;
        }

        /// <summary>
        /// Sends a information about the communication channel to the UI over http
        /// </summary>
        public void SendJobToUI()
        {
            if (_job != null)
            {
                if (_job is LiveNodePacket)
                {
                    Transmit(_job, "/NewLiveJob");
                }
                Transmit(_job, "/NewBacktestingJob");
            }
        }

        /// <summary>
        /// Send any notification with a base type of Notification.
        /// </summary>
        /// <param name="notification">The notification to be sent.</param>
        public void SendNotification(Notification notification)
        {
            var type = notification.GetType();
            if (type == typeof(NotificationEmail) || type == typeof(NotificationWeb) || type == typeof(NotificationSms))
            {
                Log.Error("Messaging.SendNotification(): Send not implemented for notification of type: " + type.Name);
                return;
            }
            notification.Send();
        }

        /// <summary>
        /// Send any message with a base type of Packet over http.
        /// </summary>
        public void Send(Packet packet)
        {
            //Packets we handled in the UX.
            switch (packet.Type)
            {
                case PacketType.Debug:
                    var debug = (DebugPacket)packet;
                    SendDebugEvent(debug);
                    break;

                case PacketType.Log:
                    var log = (LogPacket)packet;
                    SendLogEvent(log);
                    break;

                case PacketType.RuntimeError:
                    var runtime = (RuntimeErrorPacket)packet;
                    SendRuntimeErrorEvent(runtime);
                    break;

                case PacketType.HandledError:
                    var handled = (HandledErrorPacket)packet;
                    SendHandledErrorEvent(handled);
                    break;

                case PacketType.BacktestResult:
                    var result = (BacktestResultPacket)packet;
                    SendBacktestResultEvent(result);
                    break;
            }

            if (StreamingApi.IsEnabled)
            {
                StreamingApi.Transmit(_job.UserId, _job.Channel, packet);
            }
        }

        private void SendBacktestResultEvent(BacktestResultPacket packet)
        {
            Transmit(packet, "/BacktestResultEvent");
        }

        private void SendHandledErrorEvent(HandledErrorPacket packet)
        {
            Transmit(packet, "/HandledErrorEvent");
        }

        private void SendRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            Transmit(packet, "/RuntimeErrorEvent");
        }

        private void SendLogEvent(LogPacket packet)
        {
            Transmit(packet, "/LogEvent");
        }

        private void SendDebugEvent(DebugPacket packet)
        {
            Transmit(packet, "/DebugEvent");
        }



        /// <summary>
        /// Send a message to the Client using GrapeVine
        /// </summary>
        /// <param name="packet">Packet to transmit</param>
        /// <param name="resource">The resource where the packet will be sent</param>
        public static void Transmit(Packet packet, string resource)
        {
            try
            {
                var tx = JsonConvert.SerializeObject(packet);

                var request = new RESTRequest
                {
                    Method = Grapevine.HttpMethod.POST,
                    Resource = resource,
                    Payload = tx
                };

                Client.Execute(request);
            }
            catch (Exception err)
            {
                Log.Error(err, "PacketType: " + packet.Type);
            }
        }

        public bool CheckHeartBeat()
        {
            var request = new RESTRequest
            {
                Method = Grapevine.HttpMethod.GET,
                Resource = "/",
                Timeout = 1000
            };

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
}
