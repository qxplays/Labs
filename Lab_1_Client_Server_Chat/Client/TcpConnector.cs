using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Domain.ForServer;
using Domain.PublicDataContracts;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;
using InvalidOperationException = System.InvalidOperationException;


namespace Client {
    public class TcpConnector {
        public TcpClient ClientSocket;

        public TcpConnector(IMessageListener listener) {

            ClientSocket = new TcpClient();
            try {
                ClientSocket.Connect(IPAddress.Parse("127.0.0.1"), 7777);
            }
            catch (Exception) {
                ClientSocket.Connect(IPAddress.Parse("127.0.0.1"), 7777);
            }
        }


        public async Task TrySendAsync<T>(T item) where T : class, new() {
            try {
                var buff = Encoding.UTF8.GetBytes(
                    XmlSerializer<T>.SerializeObject(item));
                var ns = ClientSocket.GetStream();

                var buffer = new byte[buff.Length + 5];
                var length =
                    BitConverter.GetBytes(buff
                        .Length);
                Array.Copy(length, 0, buffer, 0, 4); 
                Array.Copy(buff, 0, buffer, 5,
                    buff.Length); 
                buffer[4] = (byte) ((IMessage) item).MessageType;

                await ns.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (InvalidOperationException) {
                var msg = item as TextMessage;
                msg.Text = "Соединение с сервером закрыто. Перезапустите программу";
                Application.Current.Dispatcher.Invoke(() =>
                    MainWindow._model.SelectedChannel.Messages.Add(msg));
            }
            catch (Exception) {
                // ignored
            }
        }
    }
}