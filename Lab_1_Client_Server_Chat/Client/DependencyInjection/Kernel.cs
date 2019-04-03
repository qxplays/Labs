using Client.DependencyInjection.NinjectModules;
using Ninject;


namespace Client.DependencyInjection {
    public class Kernel : StandardKernel {
        static Kernel _kernel;

        public static Kernel GetKernel => _kernel ?? new Kernel();


        public Kernel() : base(new ClientNinjectModule()) {
            _kernel = this;
        }
    }
}