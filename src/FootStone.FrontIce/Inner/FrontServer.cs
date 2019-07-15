using Ice;
using NLog;
using Orleans;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{

    class FrontServer
    {
        public FrontServer()
        {

        }
     
        private Communicator communicator;

        public void Init(IceOptions options, IClusterClient orleansClient)
        {         
            try
            {

                var initData = new InitializationData();
                initData.properties = Util.createProperties();
                initData.properties.load(options.ConfigFile);

                //设置日志输出              
                if (options.Logger == null)
                    options.Logger = new NLoggerI(LogManager.GetLogger("Ice"));
                initData.logger = options.Logger; 

                communicator = Util.initialize(initData);

                var adapter = communicator.createObjectAdapter("SessionFactoryAdapter");

                var properties = communicator.getProperties();

                var id = Util.stringToIdentity(properties.getProperty("Identity"));

                var serverName = properties.getProperty("Ice.ProgramName");
              
                adapter.add(new SessionFactoryI(serverName, options.FacetTypes,orleansClient), id);

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
            Task.Run(() =>
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
