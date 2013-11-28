using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Assets
{
    class ServerDiscoverer
    {
        private static bool _messageReceived = false;
        private static Server _result = null;
        public static Server DiscoverServers() {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, Protocol.serverPort);
            var udpClient = new UdpClient(ipEndPoint);

            var udpState = new UdpState();
            udpState.IpEndPoint = ipEndPoint;
            udpState.UdpClient = udpClient;

            udpClient.BeginReceive(ReceiveCallback, udpState);

            while (!_messageReceived)
            {
                Thread.Sleep(100);
            }
            return _result;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var u = ((UdpState)(ar.AsyncState)).UdpClient;
            var e = ((UdpState)(ar.AsyncState)).IpEndPoint;

            var receiveBytes = u.EndReceive(ar, ref e);
            var receiveString = Encoding.ASCII.GetString(receiveBytes);

            _messageReceived = true;
            var mySerializer = new XmlSerializer(typeof(ServerMessage));
            using (var myFileStream = new StringReader(receiveString)) {
                var serverMessage = (ServerMessage) mySerializer.Deserialize(myFileStream);
                _result = new Server(e.Address, serverMessage.Port);
            }

        }
    }
}

public class UdpState
{
    public IPEndPoint IpEndPoint;
    public UdpClient UdpClient;
}