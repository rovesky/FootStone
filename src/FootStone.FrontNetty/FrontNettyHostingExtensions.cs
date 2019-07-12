using DotNetty.Buffers;
using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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


        //public static IFSHostBuilder AddGameNetty(this IFSHostBuilder builder, Action<NettyGameOptions> config)
        //{
        //    builder.ConfigureSilo(silo =>
        //    {
        //        silo
        //        .Configure(config)
        //        .AddGrainService<NettyGameGrainService>();
        //    });
        //    return builder;
        //}

        public static IHostBuilder UseFrontNetty(this IHostBuilder builder, Action<NettyFrontOptions> config)
        {
            builder
               //.UseOrleans(silo =>
               //{
               //    silo
               //    .Configure(config)
               //    .AddGrainService<NettyGameGrainService>();
               //})
               .ConfigureServices(services =>
               {
                    // this hosted service runs the sample logic
                 //   services.AddSingleton<IHostedService, NettyFrontHostedService>();
                    // this configures the test running on this particular client
                    services.Configure(config);
               });
            return builder;
        }

        public static IHostBuilder UseGameNetty(this IHostBuilder builder, Action<NettyGameOptions> config)
        {
            builder
                //.UseOrleans(silo =>
                //{
                //    silo
                //    .Configure(config)
                //    .AddGrainService<NettyGameGrainService>();
                //})
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
