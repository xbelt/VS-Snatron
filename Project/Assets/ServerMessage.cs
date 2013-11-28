using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    class ServerMessage {
        private int _port;

        public ServerMessage(int port) {
            Port = port;
        }

        public int Port {
            get { return _port; }
            set { _port = value; }
        }
    }
}
