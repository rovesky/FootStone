using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.Grains
{
    public interface IPlayerChannel
    {
        void Send(byte[] data);
    }
}
