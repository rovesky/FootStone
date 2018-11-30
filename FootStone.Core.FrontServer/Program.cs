using FootStone.Core.FrontIce;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace FootStone.Core.FrontServer
{
    class Program
    {
        static string mysqlConnectCluster = "server=192.168.3.14;user id=root;password=654321#;database=footstone";

        static void Main(string[] args)
        {
            try
            {
                // Console.WriteLine("Hello World!");
                var network = new NetworkIce();

                network.Init(args);

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
                      .ConfigureLogging(logging => logging.AddConsole())
                      .Build();

                Global.Instance.OrleansClient = client;


                RunAsync(client, network).Wait();

                Console.ReadLine();

                StopAsync(client, network).Wait();
            }
            catch(System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

        }

        static async Task RunAsync(IClusterClient client, NetworkIce network)
        {
            network.Start();
            await client.Connect();
 
        }


        static async Task StopAsync(IClusterClient client, NetworkIce network)
        {
            await client.Close();
            network.Stop();
        }
    }
}
