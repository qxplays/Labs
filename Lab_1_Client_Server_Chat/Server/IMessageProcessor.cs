using Domain.PublicDataContracts.ForChat;


namespace Server {
    public interface IMessageProcessor {
        void Proceed(IMessage message);
    }
}   