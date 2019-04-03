using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Domain.PublicDataContracts;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using Domain.PublicDataContracts.ForUsers;
using Serilog;


namespace Server.Implementation {
    public abstract class MessageProcessorBase : IMessageProcessor {
        internal readonly ILogger _logger;

        public abstract void Proceed(IMessage message);


        protected MessageProcessorBase(ILogger logger) {
            _logger = logger;
        }


        public virtual void Send(MessageType type, IMessage msg) {
            var data = XmlSerializer<Helper>.SerializeObject(msg);
            if (!ClientManager.Channels.TryGetValue(msg.Destination.Name, out var c)) {
                c = msg.Destination;
                c.Users = new ObservableCollection<User> {msg.Sender};
                ClientManager.Channels.AddOrUpdate(c.Name, c, (s, channel1) => c);
            }
            
            foreach (var user in c.Users.ToArray()) {
                if (user.Socket.Connected)
                    try {
                        var b = Encoding.UTF8.GetBytes(data);
                        byte[] buff = new byte[b.Length + 5];
                        Array.Copy(BitConverter.GetBytes(b.Length), 0, buff, 0, 4);
                        Array.Copy(b, 0, buff, 5, b.Length);
                        buff[4] = (byte) type;
                        user.Socket.GetStream().Write(buff, 0, buff.Length);
                    }
                    catch (Exception e) {
                        NotifyUserLeft(c, msg.Sender);
                        c.Users.Remove(user);
                        _logger.Warning(e.ToString());
                    }
                else {
                    c.Users.Remove(user);
                    NotifyUserLeft(c, user);
                }
            }
        }

        
        void NotifyUserLeft(Channel channel, User u) {
            foreach (var user in channel.Users.ToArray()) {
                try {
                    var message = new TextMessage {
                        Sender = u,
                        Destination = channel,
                        Text = DateTime.Now.ToString("G") + ": Покинул канал",
                        Id = Guid.NewGuid()
                    };
                    var data = XmlSerializer<Helper>.SerializeObject(message);
                    var b = Encoding.UTF8.GetBytes(data);
                    byte[] buff = new byte[b.Length + 5];
                    Array.Copy(BitConverter.GetBytes(b.Length), 0, buff, 0, 4);
                    Array.Copy(b, 0, buff, 5, b.Length);
                    buff[4] = (byte) MessageType.Text;
                    user.Socket.GetStream().Write(buff, 0, buff.Length);
                }
                catch (Exception e) {
                    _logger.Warning(e.ToString());
                }
            }
        }
    }


    public class TextMessageProcessor : MessageProcessorBase {
        public TextMessageProcessor(ILogger logger) : base(logger) { }


        public override void Proceed(IMessage message) {
            Send(MessageType.Text, message);
        }

    }


    public class DataMessageProcessor : MessageProcessorBase {
        public DataMessageProcessor(ILogger logger) : base(logger) {
            Files = new ConcurrentDictionary<Guid, User>();
        }

        public static ConcurrentDictionary<Guid, User> Files { get; set; }

        public override void Proceed(IMessage message) {
            Send(MessageType.Data, message);
        }

        public override void Send(MessageType type, IMessage msg) {
            var message = msg as DataMessage;
            if (message.DownloadType == (DownloadType.Upload | DownloadType.Server)) {
                File.Create(message.Id.ToString()).Close();
                File.WriteAllBytes(message.Id.ToString(), message.Data);
                //сохраняем файл на диск
                _logger.Information("saving file to disk...");
                Files.AddOrUpdate(message.Id, message.Sender, (guid, user) => message.Sender);
                message.DownloadType = DownloadType.Server;
                message.Data = null;
                base.Send(type, message);

                //отложенное удаление файла через 5 минут
                Task.Run(async () => {
                    await Task.Delay(300 * 1000);
                    File.Delete(msg.Id.ToString());
                    Files.TryRemove(message.Id, out _);
                });
            }

            if (message.DownloadType == (DownloadType.Download | DownloadType.Server)) {
                Files.TryGetValue(message.Id, out var user);
                _logger.Information("Sending file...");
                var outgoingMessage = new DataMessage {
                    Data = File.ReadAllBytes(message.Id.ToString()),
                    DownloadType = message.DownloadType,
                    Destination = message.Destination,
                    Sender = user,
                    Id = Guid.NewGuid(),
                    MessageType = MessageType.Data
                };
                base.Send(type, outgoingMessage);
            }
        }
    }


    public class InfoMessageProcessor : MessageProcessorBase {
        public InfoMessageProcessor(ILogger logger) : base(logger) { }


        public override void Proceed(IMessage message) {
            if (message is InfoMessage msg) {
                Send(MessageType.Info, message);
            }
        }
    }

    public class Helper {
    }
}