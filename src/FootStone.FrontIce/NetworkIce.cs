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
             
        private Ice.Communicator communicator;

        public void Init(string configFile, IEnumerable<IServantBase> servants)
        {
            try
            {
                communicator = Ice.Util.initialize(configFile);

                var adapter = communicator.createObjectAdapter("SessionFactory");
                var properties = communicator.getProperties();
                var id = Ice.Util.stringToIdentity(properties.getProperty("Identity"));
                var serverName = properties.getProperty("Ice.ProgramName");
                adapter.add(new SessionFactoryI(serverName, servants), id);

                adapter.activate();
                Console.WriteLine("ice inited!");
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
            Task t1 = Task.Run(() =>
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
