using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using Domain.PublicDataContracts.ForUsers;
using Microsoft.Win32;


namespace Client {
    public class TextMessageProcessor : IMessageProcessor {
        public void Proceed(IMessage message) {
            if (!(message is TextMessage msg)) return;
            
            var channels = MainWindow._model.ChannelsCollection;
            var channel = channels.FirstOrDefault(x => x.Name == msg.Destination.Name) ?? msg.Destination;
            if (channel.Users == null)
                channel.Users = new ObservableCollection<User>();
            
            if (channel.Users.All(x => x.Nickname != msg.Sender.Nickname))
                Application.Current.Dispatcher.Invoke(() => channel.Users.Add(msg.Sender));
            if (msg.MessageType == MessageType.KeepAlive)
                return;
            
            if(msg.Text.ToLower().Contains("покинул"))
                Application.Current.Dispatcher.Invoke(() => channel.Users.Remove(channel.Users.FirstOrDefault(x => x.Nickname == msg.Sender.Nickname)));
            
            if (channel.Messages == null)
                Application.Current.Dispatcher.Invoke(() =>
                    channel.Messages = new ObservableCollection<Message>());
            Application.Current.Dispatcher.Invoke(() => channel.Messages.Add(msg));
            if (channels.All(x => x.Name != channel.Name)) {
                Application.Current.Dispatcher.Invoke(() => channels.Add(channel));
            }

        }
    }


    public class DataMessageProcessor : IMessageProcessor {
        public void Proceed(IMessage message) {
            if (!(message is DataMessage msg)) return;

            if (msg.DownloadType == DownloadType.Server) {
                var channels = MainWindow._model.ChannelsCollection;
                var channel = channels.FirstOrDefault(x => x.Name == msg.Destination.Name) ?? msg.Destination;
                if (channel.Users == null)
                    channel.Users = new ObservableCollection<User>();
                
                if (channel.Users.All(x => x.Nickname != msg.Sender.Nickname))
                    Application.Current.Dispatcher.Invoke(() => channel.Users.Add(msg.Sender));
                
                if (channel.Messages == null)
                    Application.Current.Dispatcher.Invoke(() =>
                        channel.Messages = new ObservableCollection<Message>());
                Application.Current.Dispatcher.Invoke(() => channel.Messages.Add(msg));
                if (channels.All(x => x.Name != channel.Name)) {
                    Application.Current.Dispatcher.Invoke(() => channels.Add(channel));
                }
            }

            if (msg.DownloadType == (DownloadType.Server | DownloadType.Download)) {
                var dialog = new SaveFileDialog();
                if ((bool) dialog.ShowDialog())
                    if (!string.IsNullOrEmpty(dialog.FileName)) {
                        dialog.OpenFile().Write(msg.Data, 0, msg.Data.Length);
                        MessageBox.Show($"Файл {dialog.FileName} сохранен");
                    }
            }

        }
    }


    public class InfoMessageProcessor : IMessageProcessor {
        public void Proceed(IMessage message) {
            throw new NotImplementedException();
        }
    }


    public class KeepAliveMessageProcessor : IMessageProcessor {
        public void Proceed(IMessage message) {
            throw new System.NotImplementedException();
        }
    }
}