namespace Domain.PublicDataContracts.ForChat {
    public interface IMessageProcessorFactory {
        IMessageProcessor GetProcessorFor(MessageType type);
    }
}