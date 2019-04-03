using Domain.PublicDataContracts.ForChat.Implementation;


namespace Client {
    public interface IMessageSender {
        void SendAsync(Message message);
    }
}