using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.FrontNetty
{
    public static class FrontNettyHostingExtensions
    {
        //public static IFSHostBuilder AddFrontNetty(this IFSHostBuilder builder, Action<NettyOptions> config)
        //{
        //    builder.ConfigureSilo(silo =>
        //    {
        //        silo
        //        .Configure(config)
        //        .AddGrainService<NettyFrontService>();
        //    });
        //    return builder;
        //}

        public static IFSClientBuilder AddFrontNetty(this IFSClientBuilder builder, Action<NettyOptions> config)
        {
            builder.ConfigureOrleans(client =>
            {
                client
                .Configure<NettyOptions>(config)
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IClientService, NettyFrontService>();
                });
            });
            return builder;
        }

    }
}
