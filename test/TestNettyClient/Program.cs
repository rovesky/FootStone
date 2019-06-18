using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNettyClient
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        static bool siloStopping = false;
        static readonly object syncLock = new object();

        static async Task RunClient(int count,int startIndex, string ip,NetworkClientNetty netty)
        {
            for (int i = startIndex; i < count+ startIndex; ++i)
            {
                await netty.ConnectNettyAsync(ip, 8007, "player" + i);
             //   await Task.Delay(5);
           }
        }

        static void Main(string[] args)
        {
            try
            { 

                int count = args.Length > 0 ? int.Parse(args[0]) : 1;
                int startIndex = args.Length > 1 ? int.Parse(args[1]) : 0;
                string  ip = args.Length > 2 ? args[2] : "192.168.0.128";
                NetworkClientNetty netty = new NetworkClientNetty();

                Console.CancelKeyPress +=  (s, a) =>
                {
                    a.Cancel = true;
                    /// Don't allow the following code to repeat if the user presses Ctrl+C repeatedly.
                    lock (syncLock)
                    {
                        if (!siloStopping)
                        {
                            siloStopping = true;

                           netty.Fini(); 
                        }
                    }
                };

                netty.Init();

                RunClient(count, startIndex, ip,netty).Wait();

                _siloStopped.WaitOne();           

            }
            catch(Exception e)
            {
                logger.Error(e);
            }
        }
    }
}
