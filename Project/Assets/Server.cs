using System;
using System.Net;

namespace Assets
{
    class Server {
        private IPAddress _ip;
        private int _port;

        public Server(IPAddress ip, int port, String name) {
        _ip = ip;
        _port = port;
        Name = name;
    }

        public string Name { get; private set; }
    } 
}
