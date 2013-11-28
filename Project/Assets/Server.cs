using System;
using System.Net;

namespace Assets
{
    class Server {
        private int _port;

        public Server(IPAddress ip, int port, String name) {
        Ip = ip;
        _port = port;
        Name = name;
    }

        public string Name { get; private set; }

        public IPAddress Ip { get; set; }
    } 
}
