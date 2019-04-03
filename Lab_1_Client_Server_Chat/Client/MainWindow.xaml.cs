using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client.DependencyInjection;
using Client.Models;
using Domain.ForServer;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using Microsoft.Win32;
using Ninject;
using Path = System.IO.Path;


namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        
        public static BindingModel _model { get; } = new BindingModel();
        //public static MainWindow Instance;

        IMessageListener _listener { get; }
        IMessageSender _messageSender { get; }

        public MainWindow() {
            InitializeComponent();
            _listener = Kernel.GetKernel.Get<IMessageListener>();
            _messageSender = Kernel.GetKernel.Get<IMessageSender>();
            ClientWindow.DataContext = this;
            Task.Factory.StartNew(async () => {
                await Task.Delay(2000);
                    //Вызов инициализации подключения к серверу + KeepAlive каждые 2 секунды
                Init();
            });
            //Instance = this;
        }


        void Init() {
            var message = new TextMessage {
                Destination = _model.SelectedChannel,
                Id = Guid.NewGuid(),
                Sender = _model.CurrentUser,
                Text = DateTime.Now.ToString("G") + ": Подключился к каналу",
                MessageType = MessageType.Text
            };

            _messageSender.SendAsync(message);

            Task.Factory.StartNew(async () => {
                while (true) {
                    foreach (var channel in _model.ChannelsCollection.ToArray()) {
                        message.MessageType = MessageType.KeepAlive;
                        message.Destination = channel;
                        _messageSender.SendAsync(message);
                        await Task.Delay(300);
                    }

                }
            });
        }


        void SendBtn_Click(object sender, RoutedEventArgs e) {
            var message = new TextMessage {
                Destination = _model.SelectedChannel,
                Id = Guid.NewGuid(),
                Sender = _model.CurrentUser,
                Text = DateTime.Now.ToString("G") + ": " + ChatTextBox.Text,
                MessageType = MessageType.Text
            };

            _messageSender.SendAsync(message);
            ChatTextBox.Text = "";
        }

        void CreateChannelBtn_Click(object sender, RoutedEventArgs e) {
            var message = new TextMessage {
                Destination = new Channel {Name = NewChannelName.Text},
                Id = Guid.NewGuid(),
                Sender = _model.CurrentUser,
                Text = DateTime.Now.ToString("G") + ": Присоединился к каналу",
                MessageType = MessageType.Text
            };

            _messageSender.SendAsync(message);
        }

        void ChatTextBox_GotFocus(object sender, RoutedEventArgs e) {
            if (ChatTextBox.Text == "Сообщение") ChatTextBox.Text = "";
        }
        

        void ChatTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                SendBtn_Click(sender, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            if ((bool) dialog.ShowDialog(this)) {
                var message = new DataMessage {
                    Data = File.ReadAllBytes(dialog.FileName),
                    Sender = _model.CurrentUser,
                    Destination = _model.SelectedChannel,
                    DownloadType = DownloadType.Server | DownloadType.Upload,
                    Id = Guid.NewGuid(),
                    MessageType = MessageType.Data,
                    Text = DateTime.Now.ToString("G") + $": Отправил файл! Дабл-клик для загрузки файла :"+Path.GetFileName(dialog.FileName)
                };
                _messageSender.SendAsync(message);
            }
        }

        private void MessageDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Messages.SelectedItem is DataMessage message) {
                if (message.DownloadType == DownloadType.Server || message.DownloadType == (DownloadType.Download|DownloadType.Server)) {
                    message.DownloadType = DownloadType.Download | DownloadType.Server;
                    _messageSender.SendAsync(message);
                }
            }
        }
    }
}
