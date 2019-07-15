using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.Core
{

    public static class FootStoneHostingExtensions
    {
        public static IHostBuilder UseOrleansClient(
            this IHostBuilder hostBuilder,
            Action<IClientBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));

            const string clientBuilderKey = "ClientBuilder";
            ClientBuilder clientBuilder;
            if (!hostBuilder.Properties.ContainsKey(clientBuilderKey))
            {
                clientBuilder = new ClientBuilder();
                hostBuilder.Properties.Add(clientBuilderKey, clientBuilder);

                hostBuilder.ConfigureServices(services => services
                        .AddSingleton<IClientBuilder>(clientBuilder)
                        //.AddSingleton<IHostedService, ClusterClientHostedService>()
                         .AddSingleton<ClusterClientHostedService>()
                         .AddSingleton<IHostedService>(_ => _.GetService<ClusterClientHostedService>())
                        .AddSingleton(_ =>
                        {
                            Console.WriteLine("_.GetService<ClusterClientHostedService>().Client");
                           return _.GetService<ClusterClientHostedService>().Client;
                            })                     
                    );
                //    hostBuilder.Configure(configureDelegate);
            }
            else
            {
                clientBuilder = (ClientBuilder)hostBuilder.Properties[clientBuilderKey];
            }
            configureDelegate(clientBuilder);
            //  clientBuilder.ConfigureSilo(configureDelegate);
            return hostBuilder;
        }
    }
}
