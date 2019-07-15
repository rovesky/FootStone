using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.FrontNetty
{
    public static class FrontNettyHostingExtensions
    {
      
        public static IHostBuilder UseFrontNetty(this IHostBuilder builder, Action<NettyFrontOptions> config)
        {
            builder           
               .ConfigureServices(services =>
               {
                    // this hosted service runs the sample logic
                    services.AddSingleton<IHostedService, NettyFrontHostedService>();
                    // this configures the test running on this particular client
                    services.Configure(config);
               });
            return builder;
        }

        public static IHostBuilder UseGameNetty(this IHostBuilder builder, Action<NettyGameOptions> config)
        {
            builder           
                .ConfigureServices(services =>
                {
                    // this hosted service runs the sample logic
                    services.AddSingleton<IHostedService, NettyGameHostedService>();
                    // this configures the test running on this particular client
                    services.Configure(config);
                });
            return builder;
        }
    }
}
