using EmbeddedServer.ProtocolWrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer
{
    static class MessageParser
    {
        static Protocol GetProtocol(string message)
        {
            if (message.Contains("Bicycle"))
            {
                return new Bicycle(message) as Protocol;
            }
            else
                return new Unknown(message) as Protocol;
        }
    }
}
