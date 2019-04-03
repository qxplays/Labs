using System;
using System.Threading;
using Domain.ForServer;
using Domain.PublicDataContracts.Events;
using Domain.PublicDataContracts.ForChat;


namespace Client.Implementation {
    public class ClientMessageListener : IMessageListener {
        readonly IMessageProcessorFactory _processorFactory;
        MessageEvent _messageEvent;


        MessageEvent IMessageListener.MessageEvent {
            get => _messageEvent;
            set => _messageEvent = value;
        }


        public void Build(CancellationToken cancellationToken) { }


        public ClientMessageListener(IMessageProcessorFactory processorFactory) {
            _processorFactory = processorFactory;
            _messageEvent += OnMessage;
        }


        public void OnMessage(MessageType type, IMessage message) {
            try {
                _processorFactory.GetProcessorFor(type).Proceed(message);
            }
            catch (Exception) {
                // ignored
            }
        }
    }
}