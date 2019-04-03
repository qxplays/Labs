using Domain.ForServer;
using Ninject.Modules;
using Serilog;
using Serilog.Core;
using Server.Implementation;
using ServerMessageListener = Server.Implementation.ServerMessageListener;


namespace Server.DependencyInjection.Modules {
    public class ServerNinjectModule:NinjectModule {
        public override void Load() {
            Bind<IMessageListener>().To<ServerMessageListener>().InSingletonScope();

            Bind<ILogger>().ToMethod(x => new LoggerConfiguration()
                                         .WriteTo.Console()
                                         .CreateLogger())
                           .InSingletonScope();
            Bind<IMessageProcessorFactory>().To<MessageProcessorFactory>();
        }
    }
}