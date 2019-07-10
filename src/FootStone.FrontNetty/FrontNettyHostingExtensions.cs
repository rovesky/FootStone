using DotNetty.Buffers;
using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using System;
using System.Text;

namespace FootStone.FrontNetty
{
    public static class FrontNettyHostingExtensions
    {
        public static IFSHostBuilder AddFrontNetty(this IFSHostBuilder builder, Action<NettyFrontOptions> config)
        {
            builder.ConfigureSilo(silo =>
            {
                silo
                .Configure(config)
                .AddGrainService<NettyFrontGrainService>();
            });
            return builder;
        }

        public static IFSFrontBuilder AddFrontNetty(this IFSFrontBuilder builder, Action<NettyFrontOptions> config)
        {
            builder.ConfigureOrleans(client =>
            {
                client
                .Configure<NettyFrontOptions>(config)
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IFrontService, NettyFrontService>();
                });
            });
            return builder;
        }


        public static IFSHostBuilder AddGameNetty(this IFSHostBuilder builder, Action<NettyGameOptions> config)
        {
            builder.ConfigureSilo(silo =>
            {
                silo
                .Configure(config)
                .AddGrainService<NettyGameGrainService>();
            });
            return builder;
        }
      
    }
}
