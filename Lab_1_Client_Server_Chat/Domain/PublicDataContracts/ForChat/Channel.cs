using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Domain.Annotations;
using Domain.PublicDataContracts.ForChat.Implementation;
using Domain.PublicDataContracts.ForUsers;


namespace Domain.PublicDataContracts.ForChat {
    public class Channel : INotifyPropertyChanged {
        ObservableCollection<User> _users;
        ObservableCollection<Message> _messages;
        public string Name { get; set; }
        

        [XmlIgnore]
        public ObservableCollection<User> Users {
            get => _users;
            set {
                _users = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public ObservableCollection<Message> Messages {
            get => _messages;
            set {
                _messages = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}