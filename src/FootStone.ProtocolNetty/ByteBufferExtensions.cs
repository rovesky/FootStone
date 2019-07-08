using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.ProtocolNetty
{
    public static class ByteBufferExtensions
    {

        public static IByteBuffer WritePoint2D(this IByteBuffer buffer, Point2D point2d)
        {
            return buffer.WriteFloat(point2d.x)
                         .WriteFloat(point2d.y);

        }
    }
}
