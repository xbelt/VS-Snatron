using System;

namespace Assets
{
    public class ServerMessage
    {
        public int _port;
        public string _name;
        public ServerMessage()
        {
            
        }
        public ServerMessage(int port, String name)
        {
            _name = name;
            _port = port;
        }
    }
}
