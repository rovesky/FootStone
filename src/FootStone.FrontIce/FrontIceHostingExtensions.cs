using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.FrontIce
{
    public static class FrontIceHostingExtensions
    {
        public static IFSHostBuilder AddFrontIce(this IFSHostBuilder builder, Action<IceOptions> config)
        {
            builder.ConfigureSilo(silo =>
            {
                silo
                .Configure(config)
                .AddGrainService<IceFrontGrainService>();
            });
            return builder;
        }

        public static IFSFrontBuilder AddFrontIce(this IFSFrontBuilder builder, Action<IceOptions> config)
        {
            builder.ConfigureOrleans(client =>
            {
                client
                .Configure(config)
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IFrontService, IceFrontService>();
                });
            });
            return builder;
        }
    }
}
