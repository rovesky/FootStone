using System;
using System.Collections.Generic;
using System.Text;

namespace SampleGameServer.Room
{
    public interface IZoneStream
    {
        void Send(byte[] data);

        void Recv(string playerId,byte[] data);
    }
}
