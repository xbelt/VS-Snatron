using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets
{
    class ServerHoster
    {
        public static int Timeout = 2000;
        public static bool IsHosting = true;

        public static void HostServer(String hostname) {
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, Protocol.ServerPort);
            var udpClient = new UdpClient();

            var message = new ServerMessage(Protocol.ServerPort, hostname);
            //Serialize message
            var serializer = new XmlSerializer(message.GetType());
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, message);
                var sendBytes4 = Encoding.ASCII.GetBytes(writer.ToString());
                (new Thread(() =>
                {
                    while (IsHosting)
                    {
                        Debug.Log("Sending host info");
                        udpClient.Send(sendBytes4, sendBytes4.Length, ipEndPoint);
                        Thread.Sleep(Timeout);
                    }
                    udpClient.Close();
                })).Start();
                
            }
        }
    }
}
