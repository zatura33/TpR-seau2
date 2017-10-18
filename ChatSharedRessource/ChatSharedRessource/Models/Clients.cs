

using System;

namespace ChatSharedRessource.Models
{
    using System.Collections.Generic;
    public class Clients
    {
        public static bool ClientsChanged { get; set; }
        public static int ClientCount { get; set; } 
        public static List<Client> MyStaticClients { get; set; }
        public  List<Client> MyClients { get; set; }

        public Clients(string myClients)
        {
            MyClients =new List<Client>();
            string[] clientsArray = myClients.Split(';');
            foreach (string client in clientsArray)
            {
                if (client != "")
                {
                    Client newClient = new Client(client);
                    MyClients.Add(newClient);
                }
            }
        }
        public Clients()
        {
            MyClients = new List<Client>();
        }
        public Clients(List<Client> myClients)
        {
            MyClients = myClients;
        }


        public static bool IsInStaticList(Client myClient)
        {
            bool isInList = false;
            foreach (Client client in MyStaticClients)
            {
                if (client.Name == myClient.Name && client.ClientIp == myClient.ClientIp)
                {
                    isInList = true;
                }
            }
            return isInList;
        }
        public static void RefreshClients(string myClients)
        {
            MyStaticClients.Clear();
            string[] clientsArray = myClients.Split(';');
            foreach (string client in clientsArray)
            {
                if (client != "")
                {
                    Client newClient = new Client(client);
                    MyStaticClients.Add(newClient);
                }
                   
            }
        }

        public static void Add(Client client)
        {
            MyStaticClients.Add(client);
            ClientCount++;
        }
        public static void Delete(Client client)
        {
            MyStaticClients.Remove(client);
        }
        public static List<Client> GetList()
        {
            return MyStaticClients;
        }

        public  string GetListString()
        {
            string jsonList=string.Empty;
            foreach (var client in MyClients)
            {
                jsonList += client.GetClientStr();
            }
          
            return jsonList;
        }
        public static string GetStaticListString()
        {
            string strList = string.Empty;
            foreach (var client in MyStaticClients)
            {
                if(client.GetClientStr() != "")
                    strList += client.GetClientStr();
            }
            return strList;
        }

    }
}