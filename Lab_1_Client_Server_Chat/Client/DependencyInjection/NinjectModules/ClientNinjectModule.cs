using Client.Implementation;
using Domain.ForServer;
using Domain.PublicDataContracts.ForChat;
using Ninject.Modules;


namespace Client.DependencyInjection.NinjectModules {
    public class ClientNinjectModule:NinjectModule {
        public override void Load() {
            Bind<IMessageListener>().To<ClientMessageListener>().InSingletonScope();
            Bind<IMessageProcessorFactory>().To<MessageProcessorFactory>().InSingletonScope();
            Bind<TcpConnector>().ToSelf().InSingletonScope();
            Bind<IMessageSender>().To<MessageSender>().InSingletonScope();
        }
    }
}