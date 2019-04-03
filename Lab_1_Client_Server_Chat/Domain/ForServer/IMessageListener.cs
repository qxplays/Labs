using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.PublicDataContracts.Events;
using Domain.PublicDataContracts.ForChat;


namespace Domain.ForServer {
    public interface IMessageListener {
        MessageEvent MessageEvent { get; set; }
        void Build(CancellationToken cancellationToken);
    }
}