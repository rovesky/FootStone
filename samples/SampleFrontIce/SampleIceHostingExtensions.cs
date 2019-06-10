using FootStone.Core;
using FootStone.FrontIce;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using System;

namespace SampleFrontIce
{
    public static class SampleIceHostingExtensions
    {
        private static void InitIceOptions(IceOptions iceOptions)
        {
            iceOptions.ConfigFile = "config";
            iceOptions.FacetTypes.Add(typeof(AccountI));
            iceOptions.FacetTypes.Add(typeof(WorldI));
            iceOptions.FacetTypes.Add(typeof(PlayerI));
            iceOptions.FacetTypes.Add(typeof(RoleMasterI));
            iceOptions.FacetTypes.Add(typeof(ZoneI));
        }        

        public static IFSHostBuilder AddFrontIce(this IFSHostBuilder builder)
        {
            builder.AddFrontIce(iceOptions =>
            {
                InitIceOptions(iceOptions);
            });
            return builder;
        }

        public static IFSClientBuilder AddFrontIce(this IFSClientBuilder builder)
        {
            builder.ConfigureOrleans(client =>
            {
                client
                .Configure<IceOptions>(iceOptions =>
                {
                    InitIceOptions(iceOptions);
                })
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IClientService, IceFrontService>();
                });
            });
            return builder;
        }

    }
}
