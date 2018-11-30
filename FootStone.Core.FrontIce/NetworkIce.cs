using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class NetworkIce
    {
        public NetworkIce()
        {

        }

        //public  void InitIce(string[] args)
        //{
        //    Thread th = new Thread(new ThreadStart(() =>
        //    {

        //        try
        //        {
        //            //
        //            // using statement - communicator is automatically destroyed
        //            // at the end of this statement
        //            //
        //            Ice.InitializationData initData = new Ice.InitializationData();

        //            initData.properties = Ice.Util.createProperties();
        //            initData.properties.setProperty("Ice.Warn.Connections", "1");
        //            initData.properties.setProperty("Ice.Trace.Network", "1");
        //            initData.properties.setProperty("SessionFactory.Endpoints", "tcp -h " + GetLocalIP() + " -p 12000");

        //            using (var communicator = Ice.Util.initialize(initData))
        //            {
        //                if (args.Length > 0)
        //                {
        //                    Console.Error.WriteLine("too many arguments");

        //                }
        //                else
        //                {


        //                    //               var adapter = communicator.createObjectAdapter("Player");
        //                    var adapter = communicator.createObjectAdapter("SessionFactory");
        //                    adapter.add(new SessionFactoryI(), Ice.Util.stringToIdentity("SessionFactory"));

        //                    adapter.activate();
        //                    Console.WriteLine("ice inited!");

        //                    communicator.waitForShutdown();

        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.Error.WriteLine(ex);

        //        }

        //    })); //创建线程                     
        //    th.Start(); //启动线程       
        //}
        private Ice.Communicator communicator;

        public void Init(string[] args)
        {
            try
            {
                communicator = Ice.Util.initialize(ref args);

                if (args.Length > 0)
                {
                    Console.Error.WriteLine("too many arguments:" + args);

                }
                else
                {
                    var adapter = communicator.createObjectAdapter("Player");
                    var properties = communicator.getProperties();
                    var id = Ice.Util.stringToIdentity(properties.getProperty("Identity"));
                    adapter.add(new PlayerI(properties.getProperty("Ice.ProgramName")), id);

                    adapter.activate();
                    Console.WriteLine("ice inited!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("ice init failed!");
                Console.Error.WriteLine(ex);

            }
        }

        public void Start()
        {
            Console.WriteLine("ice start!");
            Task t1 =Task.Run(() =>
            {
                communicator.waitForShutdown();
            });

        }

        public void Stop()
        {
            Console.WriteLine("ice shutdown!");
            communicator.shutdown();
        }

    }
}
