using Microsoft.Extensions.Configuration;
using System;
using System.IO;


namespace EmbeddedServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer(5555);
        }
    }
}
