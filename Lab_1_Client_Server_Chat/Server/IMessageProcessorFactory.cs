using Domain.ForServer;
using Domain.PublicDataContracts.ForChat;


namespace Server {
    public interface IMessageProcessorFactory {
        IMessageProcessor GetProcessorFor(MessageType type);
    }
}