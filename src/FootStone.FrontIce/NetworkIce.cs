﻿using Ice;
using NLog;
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
               
              //  Ice.Util.setProcessLogger(logger);
                var initData = new InitializationData();
                initData.properties = Util.createProperties();
                initData.properties.load(options.ConfigFile);

                //设置日志输出              
                if (options.Logger == null)
                    options.Logger = new NLoggerI(LogManager.GetLogger("Ice"));
                initData.logger = options.Logger; 

                communicator = Ice.Util.initialize(initData);

                var adapter = communicator.createObjectAdapter("SessionFactoryAdapter");

                var properties = communicator.getProperties();

                var id = Ice.Util.stringToIdentity(properties.getProperty("Identity"));

                var serverName = properties.getProperty("Ice.ProgramName");
              
                adapter.add(new SessionFactoryI(serverName, options.FacetTypes), id);

                adapter.activate();
         
                communicator.getLogger().print("Ice inited!");
            }
            catch (Ice.Exception ex)
            {
                communicator.getLogger().error("Ice init failed:" + ex.ToString());          
            }
        }

        public void Start()
        {
           
            Task t1 = Task.Run(() =>
             {
                 communicator.waitForShutdown();
             });
            communicator.getLogger().print("Ice started!");
        }

        public void Stop()
        {
            communicator.getLogger().print("Ice shutdown!");
            communicator.shutdown();
        }

    }
}
