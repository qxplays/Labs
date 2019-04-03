using Domain.PublicDataContracts.ForChat.Implementation;


namespace Client.Implementation {
    public class MessageSender:IMessageSender {
        readonly TcpConnector _connector;

        public MessageSender(TcpConnector connector) {
            _connector = connector;
        }
        public async void SendAsync(Message message) {
           await _connector.TrySendAsync(message);
        }
    }
}