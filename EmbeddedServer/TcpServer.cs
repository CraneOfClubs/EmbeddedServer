using EmbeddedServer.ProtocolWrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EmbeddedServer
{
    public class TcpServer
    {
        private TcpListener _server;
        private Boolean _isRunning;

        public TcpServer(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();

            _isRunning = true;

            LoopClients();
        }

        public void LoopClients()
        {
            while (_isRunning)
            {
                TcpClient newClient = _server.AcceptTcpClient();
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }

        public void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.ASCII);
            StreamReader sReader = new StreamReader(client.GetStream(), Encoding.ASCII);

            Boolean bClientConnected = true;
            String sData = null;

            while (bClientConnected)
            {
                sData = sReader.ReadLine();
                Protocol bicycleTest = new Bicycle(sData) as Protocol;
                var res = bicycleTest.BuildAnswer();

                Console.WriteLine("Client &gt; " + sData);
                sWriter.WriteLine(res);
                sWriter.Flush();
            }
        }
    }
}
