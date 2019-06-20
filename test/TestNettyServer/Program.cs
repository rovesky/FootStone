using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNettyServer
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        static bool siloStopping = false;
        static readonly object syncLock = new object();

        static void Main(string[] args)
        {
            try
            {
                int port = args.Length > 0 ? int.Parse(args[0]) : 8007;
                int actors  = args.Length > 1 ? int.Parse(args[1]) : 1;


                NetworkServerNetty netty = new NetworkServerNetty();


                Console.CancelKeyPress += (s, a) =>
                {
                    a.Cancel = true;
                    /// Don't allow the following code to repeat if the user presses Ctrl+C repeatedly.
                    lock (syncLock)
                    {
                        if (!siloStopping)
                        {
                            siloStopping = true;

                            Task.Run(netty.Stop);

                            _siloStopped.Set();
                        }
                    }
                };

                netty.Init();

                netty.Start(port,actors).Wait();

                _siloStopped.WaitOne();
            }
            catch(Exception e)
            {
                logger.Error(e);
            }
       
        }
    }
}
