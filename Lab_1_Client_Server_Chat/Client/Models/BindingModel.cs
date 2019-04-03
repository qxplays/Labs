using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using Domain.PublicDataContracts.ForUsers;


namespace Client.Models {
    public class BindingModel : INotifyPropertyChanged {
        #region Variables

        public event PropertyChangedEventHandler PropertyChanged;
        ObservableCollection<Channel> channelsCollection;
        private Channel _selectedChannel;

        public Channel SelectedChannel {
            get => _selectedChannel;
            set { _selectedChannel = value; OnPropertyChanged();}
        }
        
        public BindingModel() {
            CurrentUser = new User {Id = Guid.NewGuid()};
            CurrentUser.Nickname = $"User" + new Random().Next(0, 1000);
            ChannelsCollection = new ObservableCollection<Channel> {new Channel {Messages = new ObservableCollection<Message>(), Name = "General", Users = new ObservableCollection<User> {CurrentUser}}};
        }


        public User CurrentUser { get; set; }
        #endregion
        

        #region Properties
        public ObservableCollection<Channel> ChannelsCollection {
            get => channelsCollection;
            set {
                channelsCollection = value;
                OnPropertyChanged();
            }
        } 


        public virtual void OnPropertyChanged([CallerMemberName] string propName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion


        #region methods


        #endregion

    }

}