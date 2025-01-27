using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTcpServer
{
    class TcpServer 
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8001);
            server.Start();
            Console.WriteLine("Server started... Waiting for connections...");

            while(true)
            {
                //Accept client connection
                using (TcpClient client = server.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    Console.WriteLine("Client connected");

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + receivedMessage);
                }
            }
        }
    }
}
