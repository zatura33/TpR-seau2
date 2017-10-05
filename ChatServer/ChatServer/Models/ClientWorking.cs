using System.Collections.Generic;
using ChatServer;

namespace ChatSharedRessource
{
    using System.IO;
    using System.Net.Sockets;
    public class ClientWorking
    {
        private Stream ClientStream;
        private TcpClient Client;
        

        public ClientWorking(TcpClient Client)
        {
            this.Client = Client;
            ClientStream = Client.GetStream();

        }

        public void DoSomethingWithClient()
        {
            StreamWriter sw = new StreamWriter(ClientStream);
            StreamReader sr = new StreamReader(sw.BaseStream);
            sw.WriteLine(Constants.Welcome);
            sw.Flush();
            string clientData;
            try
            {
                lock (sw) { 
                    while ((clientData = sr.ReadLine()) != "exit")
                {
                    if(clientData != null)
                     ListenQueue.MyInstance().AddMessage(clientData);

                    sw.Flush();
                }
                }
            }
            finally
            {
                sw.Close();
            }
        }
    }
}