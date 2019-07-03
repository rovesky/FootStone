using NLog;
using SampleClient;
using System;

namespace FootStone.Core.Client
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();    


        static  void Main(string[] args)
        {
            try
            {

                int count = args.Length > 0? int.Parse(args[0]) : 1;
                ushort startIndex = args.Length > 1 ? ushort.Parse(args[1]) : (ushort)0;
                bool needNetty = args.Length > 2 ? bool.Parse(args[2]) : true;
                var network = new Network();
                network.Test(count, startIndex, needNetty).Wait();              

                Console.ReadLine();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }     
    }
}
