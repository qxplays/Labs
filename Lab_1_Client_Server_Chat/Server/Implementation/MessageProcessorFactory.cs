using Domain.PublicDataContracts.ForChat;
using Ninject;
using Serilog;
using Server.DependencyInjection;


namespace Server.Implementation {
    public class MessageProcessorFactory : IMessageProcessorFactory {
        readonly ILogger _logger;

        public MessageProcessorFactory(ILogger logger) {
            _logger = logger;
        }

        public IMessageProcessor GetProcessorFor(MessageType type) {
            switch (type) {
                case MessageType.Data:
                    return new DataMessageProcessor(_logger);
                case MessageType.Text:
                    return new TextMessageProcessor(_logger);
                case MessageType.Info:
                    return new InfoMessageProcessor(_logger);
                case MessageType.KeepAlive:
                    return new TextMessageProcessor(_logger);
                default: return null;
            }
        }
    }
}