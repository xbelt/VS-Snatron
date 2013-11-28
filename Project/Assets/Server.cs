using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Assets
{
    class Server {
    private IPAddress IP;
    private int port;

    public Server(IPAddress ip, int port) {
        IP = ip;
        this.port = port;
    }
} 
}
