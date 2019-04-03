using System;
using Domain.PublicDataContracts.ForUsers;


namespace Domain.PublicDataContracts.ForChat {
    public interface IMessage {
        User Sender { get; set; }
        Channel Destination { get; set; }
        Guid Id { get; set; }
        MessageType MessageType { get; set; }
    }
}