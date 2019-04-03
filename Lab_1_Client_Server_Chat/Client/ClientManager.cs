using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Domain.ForServer;
using Domain.PublicDataContracts;
using Domain.PublicDataContracts.ForChat;
using Domain.PublicDataContracts.ForChat.Implementation;


namespace Client {
    public class ClientManager {
        readonly IMessageListener _listener;
        readonly NetworkStream _stream;
        byte[] _buffer;

        public ClientManager GetInstance { get; set; }
        
        public ClientManager(TcpClient tcpClient, IMessageListener listener) {
            _listener = listener;
            _stream = tcpClient.GetStream();
            GetInstance = this;
            Task.Factory.StartNew(Read, TaskCreationOptions.LongRunning);
        }


        static void Log(byte[] buff) {
            string hex = BitConverter.ToString(buff);
            hex = hex.Replace("-", " ");
            Console.WriteLine(hex + "");
        }

        
        public void Read() {
            try {
                _buffer = new byte[4];
                _stream.BeginRead(_buffer, 0, 4, OnReceiveCallbackStatic, null);
            }
            catch (Exception) { }
        }
        
        void OnReceiveCallbackStatic(IAsyncResult result) {
            try {
                var rs = _stream.EndRead(result);
                if (rs <= 0) return;
                var length = BitConverter.ToInt32(_buffer, 0);
                Log(_buffer);
                _buffer = new byte[length + 1];
                _stream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveCallback, result.AsyncState);
            }
            catch (System.IO.IOException) { }
        }


        void OnReceiveCallback(IAsyncResult result) {
            try {
                _stream.EndRead(result);
                var buff = new byte[_buffer.Length - 1];
                Log(_buffer);
                Array.Copy(_buffer, 1, buff, 0, buff.Length);
                Log(buff);
                switch ((MessageType) Enum.Parse(typeof(MessageType), _buffer[0].ToString())) {
                    case MessageType.Data:
                        ProceedMessage<DataMessage>(MessageType.Data, buff);
                        break;
                    case MessageType.Text:
                        ProceedMessage<TextMessage>(MessageType.Text, buff);
                        break;
                    case MessageType.KeepAlive:
                        throw new NotImplementedException();
                    default:
                        break;
                }

            }
            catch (System.IO.IOException) { }
            Task.Factory.StartNew(Read, TaskCreationOptions.LongRunning);
        }


        void ProceedMessage<T>(MessageType type, byte[] buff) where T : class, new() {
            var message = (IMessage) XmlSerializer<T>.Deserialize(buff);
            _listener.MessageEvent(type, message);
        }
    }
}
