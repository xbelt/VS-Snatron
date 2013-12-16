using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    class ServerDiscoverer
	{
        public bool MessageReceived;
        private Server _result;

		private Thread ServerListener;
		
		private readonly List<Server> _servers = new List<Server>();
		// Defensive implementation
		private List<Server> publicCopy = new List<Server>();

		private bool _hasChanged;

		// Has the server list changed since the last read?
		private bool HasChanged(){
			lock (this) {
				return _hasChanged;
			}
		}

		// Before calling this, use HasChanged, please
		public List<Server> Servers {
			get {
				lock(this){
					if (_hasChanged)
						publicCopy = _servers.ToList();
					_hasChanged = false;
					return publicCopy;
				}
			}
		}

		public void StartListeningForNewServers() {
			lock (this)
			{
				if (ServerListener != null && ServerListener.IsAlive)
					return;

				ServerListener = new Thread (() =>
				{
					while (true) {
						var newServer = DiscoverServers ();
						Debug.Log ("Discovered new Server");
						var addServer = true;

						foreach (var server in _servers.Where(server => server.Ip.Equals(newServer.Ip))) {
							addServer = false;
						}
						if (addServer && newServer != null && newServer.Name != null) {
							lock(this) {
								_hasChanged = true;
								_servers.Add (newServer);
							}
						}
					}
				});
				ServerListener.Start ();
			}
		}
		
		public void StopSearching() {
			lock (this) {
				if (ServerListener == null || !ServerListener.IsAlive)
					return;
				ServerListener.Abort();
				ServerListener = null;
				_servers.RemoveAll((server) => true);
				_hasChanged = true;
			}
		}

        private Server DiscoverServers()
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

        private void ReceiveCallback(IAsyncResult ar)
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