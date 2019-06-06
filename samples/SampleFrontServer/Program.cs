using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleFrontIce;
using System;
using System.Threading.Tasks;

namespace FootStone.Core.FrontServer
{
    class Program
    {
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
       //static string mysqlConnectCluster = "server=192.168.3.28;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";

        static void Main(string[] args)
        {
            try
            {

                var client = new ClientBuilder()
                      //.UseLocalhostClustering()
                      // .UseStaticClustering(gateways)
                      .UseAdoNetClustering(options =>
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
                      })              
                      .Build();

                Global.OrleansClient = client;
                IceOptions iceOptions = new IceOptions();
                iceOptions.ConfigFile = "config";
                iceOptions.FacetTypes.Add(typeof(AccountI));
                iceOptions.FacetTypes.Add(typeof(WorldI));
                iceOptions.FacetTypes.Add(typeof(PlayerI));
                iceOptions.FacetTypes.Add(typeof(RoleMasterI));
                iceOptions.FacetTypes.Add(typeof(ZoneI));
                var network = new NetworkIce();
                network.Init(iceOptions);

                RunAsync(client, network).Wait();

                Console.ReadLine();

                StopAsync(client, network).Wait();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        static async Task RunAsync(IClusterClient client, NetworkIce network)
        {
            network.Start();
            await client.Connect();

            var grain =  client.GetGrain<IWorldGrain>("1");
            var list = await grain.GetServerList();
            Console.WriteLine("Start OK!");
        }


        static async Task StopAsync(IClusterClient client, NetworkIce network)
        {
            await client.Close();
            network.Stop();
        }
    }
}
