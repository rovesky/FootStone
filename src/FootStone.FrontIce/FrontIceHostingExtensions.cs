using FootStone.Core;
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
                .AddGrainService<IceService>();
            });
            return builder;
        }

    }
}
