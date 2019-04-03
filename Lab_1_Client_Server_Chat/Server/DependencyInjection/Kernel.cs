using Ninject;
using Ninject.Modules;
using Server.DependencyInjection.Modules;


namespace Server.DependencyInjection {
    public class Kernel:StandardKernel {
        static Kernel _kernel;

        public static Kernel GetKernel => _kernel ?? new Kernel();

        public Kernel() : base(new ServerNinjectModule()) {
            _kernel = this;
        }
    }
}