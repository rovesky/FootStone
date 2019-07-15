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
            if (hostBuilder.Properties.ContainsKey(clientBuilderKey))
                return hostBuilder;
            hostBuilder.Properties.Add(clientBuilderKey, true);


            var clientBuilder = new ClientBuilder();
            configureDelegate(clientBuilder);

            hostBuilder.ConfigureServices(services => services
                     .AddSingleton<IClientBuilder>(clientBuilder)
                     //.AddSingleton<IHostedService, ClusterClientHostedService>()
                     .AddSingleton<ClusterClientHostedService>()
                     .AddSingleton<IHostedService>(_ => _.GetService<ClusterClientHostedService>())
                     .AddSingleton(_ => _.GetService<ClusterClientHostedService>().Client)
                );

            return hostBuilder;
        }
    }
}
