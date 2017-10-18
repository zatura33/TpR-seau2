

using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using RestSharp.Serializers;

namespace ChatSharedRessource.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;


    public class Message
    {
        public string DestinationIp { get; set; }
        public string DestinationPort { get; set; }
        public string SourceIp { get; set; }
        public string SourcePort { get; set; }
        public ControlCommand ControlCommand { get; set; }
        public string Data { get; set; }
        public string Clients { get; set; }

        public Message(){   }
        public Message(string destinationIp,string destinationPort, string sourceIp,string sourcePort, string data, string clients, ControlCommand controlcommand)
        {
            DestinationIp = destinationIp;
            DestinationPort = destinationPort;
            SourceIp = sourceIp;
            SourcePort = sourcePort;
            Data = data;
            Clients = clients;
            ControlCommand = controlcommand;
        }

        

        public string EncapsulateMsg()
        {     

           
            return JsonConvert.SerializeObject(new Message(DestinationIp, DestinationPort, SourceIp,SourcePort, Data, Clients, ControlCommand), Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            
        }

        public static Message DecapsulateMsg(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
                  
        }

        public string GetData()
        {
            return Data;
        }

        public string GetDestination()
        {
            return DestinationIp;
        }

        public string GetSource()
        {
            return SourceIp;
        }

        public string GetClients()
        {
            return Clients;
        }

    }
}