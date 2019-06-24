//using DotNetty.Buffers;
//using DotNetty.Transport.Channels;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace FootStone.Core.GameServer
//{
//    class PlayerChannel : IPlayerChannel
//    {
//        private IChannel channel;
//        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

//        public PlayerChannel(IChannel channel)
//        {
//            this.channel = channel;
//        }

//        public void Recv(byte[] data)
//        {
//            throw new NotImplementedException();
//        }

//        public void Send(byte[] data)
//        {
//            logger.Debug("write data to client:"+ data.Length);
//            IByteBuffer byteBuffer = Unpooled.Buffer(data.Length);
//            byteBuffer.WriteBytes(data);
//            channel.WriteAndFlushAsync(byteBuffer);
//        }

//        public void SetZone(ZoneGrain g)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
