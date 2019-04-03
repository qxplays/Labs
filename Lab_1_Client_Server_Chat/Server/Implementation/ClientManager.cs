using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Domain.ForServer;
using Domain.PublicDataContracts;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using Domain.PublicDataContracts.ForUsers;
using Serilog;


namespace Server.Implementation {
    class ClientManager {
        public EndPoint _address;
        public TcpClient _client;
        readonly ILogger _logger;
        readonly IMessageListener _listener;
        public NetworkStream _stream;
        byte[] _buffer;
        public static List<TcpClient> ClientList = new List<TcpClient>();

        static ClientManager() {
            Channels.TryAdd("General", new Channel {
                Name = "General"
            });
        }

        public static ConcurrentDictionary<Guid, User> Users { get; set; } = new ConcurrentDictionary<Guid, User>();

        public static ConcurrentDictionary<string, Channel> Channels { get; set; } =
            new ConcurrentDictionary<string, Channel>();

        public ClientManager(TcpClient tcpClient, ILogger logger, IMessageListener listener) {
            _client = tcpClient;
            _logger = logger;
            _listener = listener;
            _stream = tcpClient.GetStream();
            _address = tcpClient.Client.RemoteEndPoint;
            ClientList.Add(_client);

            Task.Factory.StartNew(Read, TaskCreationOptions.LongRunning);

        }

        public void Read() {
            try {
                _buffer = new byte[4];
                _stream.BeginRead(_buffer, 0, 4, OnReceiveCallbackStatic, null);
            }
            catch (Exception ex) {
                ClientList.Remove(_client);
                _logger.Information(ex.ToString());
            }
        }


        void OnReceiveCallbackStatic(IAsyncResult result) {
            try {
                var rs = _stream.EndRead(result);
                if (!(rs > 0)) return;
                var length = BitConverter.ToInt32(_buffer, 0);
                _buffer = new byte[length + 1];
                _stream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveCallback, result.AsyncState);
            }
            catch (Exception ex) {
                ClientList.Remove(_client);
                _logger.Information(ex.ToString());
            }
        }


        void OnReceiveCallback(IAsyncResult result) {
            try {
                _stream.EndRead(result);
                var buff = new byte[_buffer.Length - 1];

                var msgType = (MessageType) Enum.Parse(typeof(MessageType), _buffer[0].ToString());
                if (msgType != MessageType.KeepAlive)
                    _logger.Information("Received length: " + _buffer.Length);

                Array.Copy(_buffer, 1, buff, 0, buff.Length);

                switch (msgType) {
                    case MessageType.Data:
                        ProceedMessage<DataMessage>(MessageType.Data, buff);
                        break;
                    case MessageType.Text:
                        ProceedMessage<TextMessage>(MessageType.Text, buff);
                        break;
                    case MessageType.KeepAlive:
                        ProceedMessage<TextMessage>(MessageType.Text, buff);
                        break;
                    default:
                        _logger.Error($"There is no message processor with given type {_buffer[0]}");
                        break;
                }
            }
            catch (System.IO.IOException ex) {
                _logger.Information(ex.ToString());
                ClientList.Remove(_client);
            }
            finally {
                Task.Factory.StartNew(Read, TaskCreationOptions.LongRunning);
            }

        }


        void ProceedMessage<T>(MessageType type, byte[] buff) where T : class, new() {
            var message = (IMessage) XmlSerializer<T>.Deserialize(buff);
            message.Sender.Socket = _client;
            if (Channels.TryGetValue(message.Destination.Name, out var channel)) {
                if (channel.Users == null)
                    channel.Users = new ObservableCollection<User> {message.Sender};
                if (channel.Users.All(x => x.Nickname != message.Sender.Nickname)) channel.Users.Add(message.Sender);
            }
            else {
                channel = new Channel
                    {Name = message.Destination.Name, Users = new ObservableCollection<User> {message.Sender}};
            }

            Channels.AddOrUpdate(message.Destination.Name, channel, (s, channel1) => channel);

            message.Sender.Socket = _client;
            Users.AddOrUpdate(message.Sender.Id, message.Sender, (guid, user) => message.Sender);
            _listener.MessageEvent(type, message);
        }

        ~ClientManager() {
            try {
                foreach (var c in ClientList) c.Close();
                ClientList.Clear();
            }
            finally {
                _logger.Information("Shutdown...");
            }
        }


    }
}
