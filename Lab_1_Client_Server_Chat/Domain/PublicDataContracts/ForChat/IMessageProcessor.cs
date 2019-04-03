namespace Domain.PublicDataContracts.ForChat {
    public interface IMessageProcessor {
        void Proceed(IMessage message);
    }
}   