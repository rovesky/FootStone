using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.FrontIce
{
    public static class FrontIceHostingExtensions
    {
       
        public static IHostBuilder UseFrontIce(this IHostBuilder builder, Action<IceOptions> config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var key = "IsUseFrontIce";
            if (builder.Properties.ContainsKey(key))
                return builder;
            builder.Properties.Add(key, true);

            builder
                .ConfigureServices(services =>
                {
                    // this hosted service runs the sample logic
                    services.AddSingleton<IHostedService, IceFrontHostedService>();
                    // this configures the test running on this particular client
                    services.Configure(config);
                });
            return builder;
        }
    }
}
