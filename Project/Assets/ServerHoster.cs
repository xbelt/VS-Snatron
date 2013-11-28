using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Assets
{
    class ServerHoster
    {
        public static void hostServer(String hostname) {
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, Protocol.serverPort);
            var udpClient = new UdpClient();
            var sendBytes4 = Encoding.ASCII.GetBytes();
            udpClient.Send(sendBytes4, sendBytes4.Length, ipEndPoint);
        }
    }
}
