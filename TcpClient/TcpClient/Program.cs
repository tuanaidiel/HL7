using System;
using System.Net.Sockets;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTcpClient
{
    class TcpClientProgram
    {
        static void Main(string[] args)
        {
            //Connect to server
            using (TcpClient client = new TcpClient ("127.0.0.1", 8001))
            using (NetworkStream stream = client.GetStream())
            {
                string message = "Hello Tuan Muhammad Aidiel!";
                byte[] data = Encoding.UTF8.GetBytes(message);

                stream.Write(data, 0, data.Length);
                Console.WriteLine("Message sent to server");
                Console.ReadLine();
            }
        }
    }
}
