using FootStone.Client;
using NLog;
using SampleClient;
using System;
using System.Threading.Tasks;

namespace FootStone.Core.Client
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            try
            {
                int count = args.Length > 0 ? int.Parse(args[0]) : 1;
                ushort startIndex = args.Length > 1 ? ushort.Parse(args[1]) : (ushort)0;
                bool needNetty = args.Length > 2 ? bool.Parse(args[2]) : true;

                string ip = args.Length > 3 ?args[3] : "192.168.0.128";
                int port = args.Length > 4 ? int.Parse(args[4]) : 4061;


                //OldTest(args);
                NewTest(ip,port,count, startIndex, needNetty);

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static async Task NewTest(string ip,int port,int count, ushort startIndex, bool needNetty)
        {
            var network = new NetworkNew();
            await network.Test(ip,port,count, startIndex, needNetty);
        }


        static void OldTest(int count, ushort startIndex, bool needNetty)
        {
            var network = new Network();
            network.Test(count, startIndex, needNetty).Wait();
         
        }
    }
}
