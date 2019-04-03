using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Domain.PublicDataContracts.ForUsers;


namespace Domain.PublicDataContracts.ForChat.Implementation {
    public class TextMessage : Message {

    }


    public class DataMessage : Message {
        public string FileName { get; set; }
        public DownloadType DownloadType { get; set; }
        public byte[] Data { get; set; }
    }


    public class InfoMessage : Message { }


    public class Message : IMessage
    {
        public string Text { get; set; }

        public User Sender { get; set; }
        public Channel Destination { get; set; }
        public Guid Id { get; set; }
        public MessageType MessageType { get; set; }
    }

}