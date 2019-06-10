using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleFrontIce;
using System;

namespace FootStone.Core.FrontServer
{
    class Program
    {
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        //static string mysqlConnectCluster = "server=192.168.3.28;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";

        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {

                var client = new FSClientBuilder()
                    .ConfigureOrleans( orleansBuilder => {
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
                          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPlayerGrain).Assembly).WithReferences())
                          .ConfigureLogging(logging =>
                          {
                              logging.AddProvider(new NLogLoggerProvider());
                          })                        

                          .AddSimpleMessageStreamProvider("Zone", cfg =>
                          {
                              cfg.FireAndForgetDelivery = true;
                          });
                      })
                      .AddFrontIce()
                      .Build();

                Global.OrleansClient = client.ClusterClient;           
       
                client.StartAsync().Wait();

                logger.Info("Front Server Started!");
             
                do
                {
                    string exit = Console.ReadLine();
                    if (exit.Equals("exit"))
                    {
                        client.StopAsync().Wait();
                        break;
                    }
                } while (true);

            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }      
    }
}
