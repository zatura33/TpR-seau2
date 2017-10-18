using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ChatSharedRessource.Assets;
using ChatSharedRessource.Models;
using Newtonsoft.Json;

namespace ChatClient
{
    public class ClientAsynchListener
    {

        private static TcpListener serverSocket;


        public static void StartServer()

        {
            Process currentProcess = Process.GetCurrentProcess();

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, currentProcess.Id/7);

            serverSocket = new TcpListener(ipEndPoint);

            serverSocket.Start();

            Console.WriteLine("Asynchonous server socket is listening at: " + ipEndPoint.Address.ToString());

            WaitForClients();

        }

        private static void WaitForClients()

        {

            serverSocket.BeginAcceptTcpClient(new System.AsyncCallback(OnClientConnected), null);

        }

        private static void OnClientConnected(IAsyncResult asyncResult)

        {

            try

            {

                TcpClient clientSocket = serverSocket.EndAcceptTcpClient(asyncResult);

                if (clientSocket != null)

                    Console.WriteLine("Received connection request from: " +
                                      clientSocket.Client.RemoteEndPoint.ToString());

                HandleClientRequest(clientSocket);

            }

            catch

            {

              //  throw;

            }

            WaitForClients();

        }

        private static void HandleClientRequest(TcpClient clientSocket)

        {

            NetworkStream receivestring = clientSocket.GetStream();
            BinaryReader reader = new BinaryReader(clientSocket.GetStream());
            string returndata = reader.ReadString();
            Message message = JsonConvert.DeserializeObject<Message>(returndata, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            lock (ListenQueues.MyInstance())
            {
                ListenQueues.MyInstance().AddMessage(message);
            }
        }
    }
}
