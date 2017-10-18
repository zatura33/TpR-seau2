using System;
using System.Net;
using System.Net.Sockets;
using ChatSharedRessource.Assets;

namespace ChatSharedRessource.Models
{
    public abstract class ControlCommand
    {
        public string Value { get; set; }
        public abstract void ServerTreatment(Message message);
        public abstract void ClientTreatment(Message message);

        public string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            throw new Exception(Constants.IpNotFound);
        }

        public Client FindClient(Client client)
        {
            Client findedClient = null;
            foreach (var clientFromList in Clients.MyStaticClients)
            {
                if (clientFromList.Name == client.Name &&
                    clientFromList.ClientIp == client.ClientIp &&
                    clientFromList.ClientPort == client.ClientPort)
                    findedClient = clientFromList;
            }
            return findedClient;
        }
    }


        public class ClientList : ControlCommand
        {
            public ClientList()
            {
                Value = Constants.List;
            }

            public void MulticastNewClientList()
            {
                var sendList = new Clients(Clients.GetStaticListString());

                foreach (var client in sendList.MyClients)
                {
                    var sendNewListMessage = new Message
                    {
                        DestinationIp = client.ClientIp,
                        DestinationPort = client.ClientPort,
                        SourceIp = GetLocalIpAddress(),
                        SourcePort = Constants.ServerListenerPort.ToString(),
                        Data = Constants.Connect,
                        Clients = Clients.GetStaticListString(),
                        ControlCommand = new ClientList()
                    };
                    var serveResponse = new Client(sendNewListMessage.DestinationIp, sendNewListMessage.DestinationPort,
                        Constants.ServerName,
                        GetLocalIpAddress(), Clients.ClientCount, true);
                    serveResponse.SendMessage(sendNewListMessage);
                }
            }

            public override void ServerTreatment(Message message)
            {
                /* lock (ListenQueues.MyInstance())
                 {
                     ListenQueues.MyInstance().AddTextMessage(message.Clients);
                 }*/
            }

            public override void ClientTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    Clients.RefreshClients(message.Clients);
                    //AddClientToList(message);
                    Clients.ClientsChanged = true;
                }
            }
        }

        public class Connect : ControlCommand
        {
            public Connect()
            {
                Value = Constants.Connect;
            }

            public override void ServerTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    Clients.ClientsChanged = true;
                    Client newClient =new Client(message.Clients);
                   if (!Clients.IsInStaticList(newClient))
                        {
                        AddClientToList(message);
                        }                   
                    SendConnexionMessage(message);
                    var clientConnecting = new Client(message.Clients);
                    //Send to Ui Box
                    ListenQueues.MyInstance()
                        .AddTextMessage(string.Format(Constants.ClientIsConnecting, clientConnecting.Name));
                    var allClients = new ClientList();
                    allClients.MulticastNewClientList();
                }
            }


            private void AddClientToList(Message message)
            {
                if (message != null)
                {
                    Clients.MyStaticClients.Add(new Client(message.Clients));
                    var test = Clients.GetStaticListString();
                    Clients.ClientsChanged = true;
                }
            }

            private void SendConnexionMessage(Message message)
            {
                var server = new Client(GetLocalIpAddress(), Constants.ServerListenerPort.ToString(),
                    Constants.ServerName,
                    GetLocalIpAddress(), 0, true);
                var connectMessage = new Message
                {
                    DestinationIp = message.SourceIp,
                    DestinationPort = message.SourcePort,
                    SourceIp = message.DestinationIp,
                    SourcePort = message.DestinationPort,
                    Data = Constants.Connect,
                    Clients = Clients.GetStaticListString(),
                    ControlCommand = new Connect()
                };
                var serveResponse = new Client(message.DestinationIp, message.DestinationPort, Constants.ServerName,
                    GetLocalIpAddress(), Clients.ClientCount, true);
                serveResponse.SendMessage(connectMessage);
            }


            public override void ClientTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    Clients.RefreshClients(message.Clients);
                    ListenQueues.MyInstance().AddTextMessage(Constants.Welcome);
                    //AddClientToList(message);
                    Clients.ClientsChanged = true;
                    Client.CurrentConnection.IsConnected = true;
                }
            }
        }

        public class Send : ControlCommand
        {
            public Send()
            {
                Value = Constants.Send;
            }

            public override void ServerTreatment(Message message)
            {
            lock (ListenQueues.MyInstance())
            {
                string[] clientSendingMsg = message.Data.Split('-');
                ListenQueues.MyInstance().AddTextMessage(clientSendingMsg[0]+ Constants.SendingMessag);

            }
                SendMultipleMessage(message);
            }

            public void SendMultipleMessage(Message message)
            {
            Clients clients= new Clients(message.Clients);
            foreach (Client client in clients.MyClients)
            {
                Message sendNewListMessage;
                if (Clients.IsInStaticList(client))
                {
                     sendNewListMessage = new Message
                    {
                        DestinationIp = client.ClientIp,
                        DestinationPort = client.ClientPort,
                        SourceIp = GetLocalIpAddress(),
                        SourcePort = Constants.ServerListenerPort.ToString(),
                        Data = message.Data,
                        Clients = Clients.GetStaticListString(),
                        ControlCommand = new Send()
                     };
                }
                else
                {
                    sendNewListMessage = new Message
                    {
                        DestinationIp = message.SourceIp,
                        DestinationPort = message.SourcePort,
                        SourceIp = GetLocalIpAddress(),
                        SourcePort = Constants.ServerListenerPort.ToString(),
                        Data = string.Format(Constants.ClientIsNotConnected, client.Name),
                        Clients = Clients.GetStaticListString(),
                        ControlCommand = new Send()
                    };
                }

                var serveResponse = new Client(sendNewListMessage.DestinationIp, sendNewListMessage.DestinationPort,
                    Constants.ServerName,
                    GetLocalIpAddress(), Clients.ClientCount, true);
                serveResponse.SendMessage(sendNewListMessage);                
            }
        }

            public override void ClientTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    ListenQueues.MyInstance().AddTextMessage(message.Data);
                }
            }
        }
        public class Quit : ControlCommand
        {
            public Quit()
            {
                Value = Constants.Quit;
            }


            public override void ServerTreatment(Message message)
            {
                Clients.ClientsChanged = true;
                var disconnectingClient = FindClient(new Client(message.Clients));
                Clients.MyStaticClients.Remove(disconnectingClient);

                var refreshedList = new ClientList();
                refreshedList.MulticastNewClientList();
                lock (ListenQueues.MyInstance())
                {
                    ListenQueues.MyInstance()
                        .AddTextMessage(string.Format(Constants.ClientIsDisconnected, disconnectingClient.Name));
                }
            }

            public override void ClientTreatment(Message message)
            {
            }
        }
    }