using DotNetty.Buffers;
using FootStone.ProtocolNetty;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleClient
{


    [Serializable]
    public struct Move
    {
        public static ushort Type = 0x03;

        public byte direction;
        public byte speed;
        public Point2D point;

        public void Encoder(IByteBuffer buffer)
        {
            buffer.WriteUnsignedShort(Type)
                .WriteByte(direction)
                .WriteByte(speed)
                .WritePoint2D(point);
        }
    }
}
