using System.Net.Sockets;
using Newtonsoft.Json;

namespace ChatSharedRessource.Models
{
    using System;
    using System.IO;
    using ChatSharedRessource.Models;
    public class AsynchClient
    {

     

            public TcpClient ClientSocket = new System.Net.Sockets.TcpClient();

            //NetworkStream networkStream;

            public void Connect(string ipAddress, int port)

            {

                ClientSocket.Connect(ipAddress, port);

            }

            public void Send(Message message)

            {
                BinaryWriter writer = new BinaryWriter(ClientSocket.GetStream());
                string test = message.EncapsulateMsg();
                Console.WriteLine(test);
                writer.Write(message.EncapsulateMsg());
             }

            public void Close()

            {
                ClientSocket.Close();
            }

           

    }
    
}