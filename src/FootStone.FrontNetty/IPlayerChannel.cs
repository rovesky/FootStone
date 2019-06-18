using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.FrontNetty
{
    public interface IPlayerChannel
    {
        void Send(byte[] data);
    }
}
