using System;
using System.Net;
using System.Net.Sockets;
using ChatSharedRessource.Assets;

namespace ChatSharedRessource.Models
{
    // All Messages and command are encapsulated in controlCommand Object
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

        // ClientList command refresh the clients for all entity
        public class ClientList : ControlCommand
        {
            public ClientList()
            {
                Value = Constants.List;
            }
            // Send newlientList to all client via server the server get it too.
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
            }
            // Refresh ClientList and  Server ClientList 
            public override void ClientTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    Clients.RefreshClients(message.Clients);
                    Clients.ClientsChanged = true;
                }
            }
        }
        // Connect command is used to connect client on server
        public class Connect : ControlCommand
        {
            public Connect()
            {
                Value = Constants.Connect;
            }
            // On server side if client not connected connect the client and refresh list
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
                    Client clientConnecting = new Client(message.Clients);
                    //Send to Ui Box
                    ListenQueues.MyInstance()
                        .AddTextMessage(string.Format(Constants.ClientIsConnecting, clientConnecting.Name));
                    ClientList allClients = new ClientList();
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
            //for clients
            private void SendConnexionMessage(Message message)
            {
                Client server = new Client(GetLocalIpAddress(), Constants.ServerListenerPort.ToString(),
                    Constants.ServerName,
                    GetLocalIpAddress(), 0, true);
                Message connectMessage = new Message
                {
                    DestinationIp = message.SourceIp,
                    DestinationPort = message.SourcePort,
                    SourceIp = message.DestinationIp,
                    SourcePort = message.DestinationPort,
                    Data = Constants.Connect,
                    Clients = Clients.GetStaticListString(),
                    ControlCommand = new Connect()
                };
                Client serveResponse = new Client(message.DestinationIp, message.DestinationPort, Constants.ServerName,
                    GetLocalIpAddress(), Clients.ClientCount, true);
                serveResponse.SendMessage(connectMessage);
            }
            // Tell Client that he's connected
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
        // Use to send text message between clients
        public class Send : ControlCommand
        {
            public Send()
            {
                Value = Constants.Send;
            }
            // Server transfer the data to clients in a client list with the message
            public override void ServerTreatment(Message message)
            {
            lock (ListenQueues.MyInstance())
            {
                string[] clientSendingMsg = message.Data.Split('-');
                ListenQueues.MyInstance().AddTextMessage(clientSendingMsg[0]+ Constants.SendingMessag);

            }
                SendMultipleMessage(message);
            }
            // Use to send message to one or multiple clients
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
            //When client receive message it is put in his listening Queue
            public override void ClientTreatment(Message message)
            {
                lock (ListenQueues.MyInstance())
                {
                    ListenQueues.MyInstance().AddTextMessage(message.Data);
                }
            }
        }
        // Quit command is used to disconnect client on server
        public class Quit : ControlCommand
        {
            public Quit()
            {
                Value = Constants.Quit;
            }
            public override void ServerTreatment(Message message)
            {
                Clients.ClientsChanged = true;
                Client disconnectingClient = FindClient(new Client(message.Clients));
                Clients.MyStaticClients.Remove(disconnectingClient);
                ClientList refreshedList = new ClientList();
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