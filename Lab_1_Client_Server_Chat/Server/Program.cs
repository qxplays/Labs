using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.ForServer;
using Ninject;
using Server.DependencyInjection;
using Server.Implementation;


namespace Server {
    class Program {
        static CancellationTokenSource token = new CancellationTokenSource();

        static void Main(string[] args) {
            var listener = Kernel.GetKernel.Get<IMessageListener>();
            SetConsoleCtrlHandler(ServerStop, true);
            listener.Build(token.Token);

            while (Console.ReadLine() is string cmd && !cmd.ToLower().Contains("stop")) {
                if (cmd.Contains("kick")) {
                    var user = ClientManager.Users.Values.FirstOrDefault(x => x.Nickname == (cmd.Split(' ')[1] ?? ""));
                    if (user == null)
                        continue;
                    user.Socket.Close();
                    ClientManager.ClientList.Remove(user.Socket);
                    ClientManager.Users.TryRemove(user.Id, out _);
                }
            }

            token.Cancel();
        }
        
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(Handler handler, bool b); 


        public delegate bool Handler(Types type);


        public enum Types {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }


        static bool ServerStop(Types type) {
            token.Cancel();

            return true;
        }
    }
}
