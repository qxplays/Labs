using Domain.ForServer;
using Domain.PublicDataContracts.ForChat;


namespace Domain.PublicDataContracts.Events {

    public delegate void MessageEvent(MessageType type, IMessage message);
}