namespace ChatSharedRessource.Models
{
    using System;
    using Newtonsoft.Json;

    public class Client
    {
        public static Client CurrentConnection { get; set; }
        public int ConnectionId { get; set; }
        public string ClientIp { get; set; }
        public string ClientPort { get; set; }
        public string Name { get; set; }
        public string ServerAdress { get; set; }
        public string TextMessages { get; set; }
        public bool IsConnected { get; set; }
        public bool IsSelected{ get; set; }
        [JsonConstructor]
        public Client(string clienIp,string clienPort, string name, string serverAdress, int connectionId,bool isConnected)
        {
            ClientIp = clienIp;
            ClientPort = clienPort;
            Name = name;
            ServerAdress = serverAdress;
            ConnectionId = connectionId;
            IsConnected = isConnected;
        }
        public string GetClientStr()
        {        
                return ClientIp + "," + ClientPort + "," + Name + "," + ServerAdress + "," + ConnectionId+
                       ","+  IsConnected + ",;";          
        }
        public Client(string fullClientString)
        {
                string[] clientArray = fullClientString.Split(',');
                ClientIp = clientArray[0];
                ClientPort = clientArray[1];
                Name = clientArray[2];
                ServerAdress = clientArray[3];
                ConnectionId = Int32.Parse(clientArray[4]);
                IsConnected = bool.Parse(clientArray[5]);
        }

        public void SendMessage(Message message)
        {
            AsynchClient client = new AsynchClient();
            client.Connect(message.GetDestination(), Int32.Parse(message.DestinationPort));
            client.Send(message);          
        }

    }
}