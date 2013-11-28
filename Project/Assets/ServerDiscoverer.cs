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
        public static bool MessageReceived;
        private static Server _result;
        public static Server DiscoverServers()
        {
            MessageReceived = false;
            var ipEndPoint = new IPEndPoint(IPAddress.Any, Protocol.ServerPort);
            var udpClient = new UdpClient(ipEndPoint);

            var udpState = new UdpState();
            udpState.IpEndPoint = ipEndPoint;
            udpState.UdpClient = udpClient;
            udpClient.BeginReceive(ReceiveCallback, udpState);

            while (!MessageReceived)
            {
                Thread.Sleep(100);
            }
            udpClient.Close();
            return _result;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var u = ((UdpState)(ar.AsyncState)).UdpClient;
            var e = ((UdpState)(ar.AsyncState)).IpEndPoint;

            var receiveBytes = u.EndReceive(ar, ref e);
            var receiveString = Encoding.ASCII.GetString(receiveBytes);

            MessageReceived = true;
            var mySerializer = new XmlSerializer(typeof(ServerMessage));
            using (var myFileStream = new StringReader(receiveString)) {
                var serverMessage = (ServerMessage) mySerializer.Deserialize(myFileStream);
                _result = new Server(e.Address, serverMessage._port, serverMessage._name);
            }

        }
    }
}

public class UdpState
{
    public IPEndPoint IpEndPoint;
    public UdpClient UdpClient;
}