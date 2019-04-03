using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Domain.ForServer;
using Domain.PublicDataContracts.Events;
using Domain.PublicDataContracts.ForChat;
using Serilog;
using Server.DependencyInjection;


namespace Server.Implementation {
    public class ServerMessageListener : IMessageListener {
        readonly ILogger _logger;
        readonly IMessageProcessorFactory _processorFactory;
        MessageEvent _messageEvent;

        public ServerMessageListener(ILogger logger, IMessageProcessorFactory processorFactory) {
            _logger = logger;
            _processorFactory = processorFactory;
        }

        MessageEvent IMessageListener.MessageEvent {
            get => _messageEvent;
            set => _messageEvent = value;
        }
        

        public void Build(CancellationToken cancellationToken) {
            Task.Factory.StartNew(async () => {
                _messageEvent += OnMessage;
                var srv = new TCPServer(_logger, this, cancellationToken);
                while (!cancellationToken.IsCancellationRequested) {
                    await Task.Delay(3000, cancellationToken);
                }

                srv.Destroy();
            }, TaskCreationOptions.LongRunning);
        }

        public void OnMessage(MessageType type, IMessage message) {
            try {
                _processorFactory.GetProcessorFor(type).Proceed(message);
            }
            catch (Exception e) {
                _logger.Information(e.ToString());
            }
        }
    }


    public class TCPServer {
        IMessageListener _msgListener { get; }
        static ILogger _logger;
        private readonly CancellationToken _token;


        protected TcpListener _listener;

        public TCPServer(ILogger logger, IMessageListener msgListener, CancellationToken token) {
            _msgListener = msgListener;
            _logger = logger;
            _token = token;
            _listener = new TcpListener(IPAddress.Any, 7777);
            _listener.Start();
            _logger.Information("Сервер запущен");
            
            while (true) {
                var clientSocket = _listener.AcceptTcpClient();
                ClientAccept(clientSocket);
            }
        }


        void ClientAccept(TcpClient client) {
            new ClientManager(client, _logger, _msgListener);
            var log = client.Client.RemoteEndPoint + " подключился";
            _logger.Information(log);
        }

        public void Destroy() {
            _listener.Stop();
        }
    }
}

