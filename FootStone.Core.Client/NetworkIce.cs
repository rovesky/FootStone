using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.Client
{
    class NetworkIce
    {
        public void Init()
        {
            try
            {
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                // initData.properties.setProperty("Ice.ACM.Client", "0");
                // initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "0");
                initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h localhost -p 10000");

                //
                // using statement - communicator is automatically destroyed
                // at the end of this statement
                //
                using (var communicator = Ice.Util.initialize(initData))
                {
                 //   var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));

                   
                    communicator.waitForShutdown();

                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
        }
    }
}
