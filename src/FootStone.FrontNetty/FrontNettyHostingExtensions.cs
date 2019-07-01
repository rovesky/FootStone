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

        public static IFSClientBuilder AddFrontNetty(this IFSClientBuilder builder, Action<NettyFrontOptions> config)
        {
            builder.ConfigureOrleans(client =>
            {
                client
                .Configure<NettyFrontOptions>(config)
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IClientService, NettyFrontService>();
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

        //public static IByteBuffer WriteStringShort(this IByteBuffer buffer, string value, Encoding encoding)
        //{
        //    buffer.WriteUnsignedShort((ushort)value.Length).
        //        WriteString(value, encoding);
        //    return buffer;
        //}

        //public static string ReadStringShort(this IByteBuffer buffer,  Encoding encoding)
        //{
        //    var len = buffer.ReadUnsignedShort();
        //    return buffer.ReadString(len, encoding);
        //}

        //public static IByteBuffer WriteStringShortUtf8(this IByteBuffer buffer, string value)
        //{
        //    buffer.WriteUnsignedShort((ushort)value.Length).
        //        WriteString(value, Encoding.UTF8);
        //    return buffer;
        //}

        //public static string ReadStringShortUtf8(this IByteBuffer buffer)
        //{
        //    var len = buffer.ReadUnsignedShort();
        //    return buffer.ReadString(len, Encoding.UTF8);
        //}
    }
}
