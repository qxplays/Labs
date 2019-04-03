using Domain.PublicDataContracts.ForChat;


namespace Client {
    public class MessageProcessorFactory : IMessageProcessorFactory {
        public IMessageProcessor GetProcessorFor(MessageType type) {
            switch (type) {
                case MessageType.Data:
                    return new DataMessageProcessor();
                case MessageType.Text:
                    return new TextMessageProcessor();
                case MessageType.Info:
                    return new InfoMessageProcessor();
                case MessageType.KeepAlive:
                    return new KeepAliveMessageProcessor();
                default: return null;
            }
        }
    }
}