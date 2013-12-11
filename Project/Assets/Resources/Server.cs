using System;
using System.Net;

namespace Assets
{
    class Server {
        public Server(IPAddress ip, int port, String name) {
        Ip = ip;
        Port = port;
        Name = name;
    }

        public string Name { get; private set; }

        public IPAddress Ip { get; set; }

        public int Port { get; set; }
    } 
}
