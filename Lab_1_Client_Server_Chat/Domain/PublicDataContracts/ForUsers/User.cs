using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Domain.Annotations;


namespace Domain.PublicDataContracts.ForUsers {
    public class User:INotifyPropertyChanged {
        string _nickname;


        [IgnoreDataMember]
        [XmlIgnore]
        public TcpClient Socket { get; set; }


        public string Nickname {
            get => _nickname;
            set { _nickname = value; OnPropertyChanged(); }
        }


        public Guid Id { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}