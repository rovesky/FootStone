using FootStone.FrontIce;
using FootStone.FrontNetty;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleFrontIce;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Core.FrontServer
{
    class Program
    {
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        //static string mysqlConnectCluster = "server=192.168.3.28;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";

        private static Logger logger = LogManager.GetCurrentClassLogger();
        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        static bool siloStopping = false;
        static readonly object syncLock = new object();

        static void Main(string[] args)
        {
            try
            {

                IFSClient client = null;
                Console.CancelKeyPress += (s, a) =>
                {
                    a.Cancel = true;
                    /// Don't allow the following code to repeat if the user presses Ctrl+C repeatedly.
                    lock (syncLock)
                    {
                        if (!siloStopping)
                        {
                            siloStopping = true;
                            Task.Run(async () =>
                            {
                                await client.StopAsync();
                                _siloStopped.Set();
                            }).Ignore();
                        }
                    }
                };
                do
                {
                    try
                    {
                        client = new FSClientBuilder()
                        .ConfigureOrleans(orleansBuilder =>
                        {
                            //.UseLocalhostClustering()
                            // .UseStaticClustering(gateways)
                            orleansBuilder.UseAdoNetClustering(options =>
                              {
                                  options.ConnectionString = mysqlConnectCluster;
                                  options.Invariant = "MySql.Data.MySqlClient";
                              })
                              .Configure<ClusterOptions>(options =>
                              {
                                  options.ClusterId = "lsj";
                                  options.ServiceId = "FootStone";
                              })
                              .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccountGrain).Assembly).WithReferences())
                              .ConfigureLogging(logging =>
                              {
                                  logging.AddProvider(new NLogLoggerProvider());
                              });

                            //.AddSimpleMessageStreamProvider("Zone", cfg =>
                            //{
                            //    cfg.FireAndForgetDelivery = true;
                            //});
                        })
                        //添加ICE支持
                        .AddFrontIce()
                        //添加Netty支持
                        .AddFrontNetty(options =>
                        {                        
                            options.FrontPort = 8007;
                            options.GamePort = 8017;
                        })
                        .Build();                 

                        client.StartAsync().Wait();
                        break;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        logger.Info("start failed ,try again!");
                        Task.Delay(3000).Wait();
                    }
                }
                while (true);              

                logger.Info("Front Server Started!");

                _siloStopped.WaitOne();        

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }      
    }
}
