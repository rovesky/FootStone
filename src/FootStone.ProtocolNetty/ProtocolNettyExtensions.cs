using DotNetty.Buffers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;

namespace FootStone.ProtocolNetty
{
    public static class ProtocolNettyExtensions
    {       

        public static IByteBuffer WriteStringShort(this IByteBuffer buffer, string value, Encoding encoding)
        {
            buffer.WriteUnsignedShort((ushort)value.Length).
                WriteString(value, encoding);
            return buffer;
        }

        public static string ReadStringShort(this IByteBuffer buffer,  Encoding encoding)
        {
            var len = buffer.ReadUnsignedShort();
            return buffer.ReadString(len, encoding);
        }

        public static IByteBuffer WriteStringShortUtf8(this IByteBuffer buffer, string value)
        {
            buffer.WriteUnsignedShort((ushort)value.Length).
                WriteString(value, Encoding.UTF8);
            return buffer;
        }

        public static string ReadStringShortUtf8(this IByteBuffer buffer)
        {
            var len = buffer.ReadUnsignedShort();
            return buffer.ReadString(len, Encoding.UTF8);
        }
    }
}
