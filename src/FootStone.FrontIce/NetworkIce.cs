using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class NetworkIce
    {
        public NetworkIce()
        {

        }
             
        private Ice.Communicator communicator;

        public void Init(IceOptions options)
        {
            try
            {
                communicator = Ice.Util.initialize(options.ConfigFile);              

                var adapter = communicator.createObjectAdapter("SessionFactoryAdapter");         

                var properties = communicator.getProperties();
              
                var id = Ice.Util.stringToIdentity(properties.getProperty("Identity"));
                var serverName = properties.getProperty("Ice.ProgramName");             

                adapter.add(new SessionFactoryI(serverName, options.FacetTypes, communicator), id);

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
