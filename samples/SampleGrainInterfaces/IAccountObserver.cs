using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.GrainInterfaces
{
    public interface IAccountObserver : IGrainObserver
    {
        void AccountLogined(string sessionId);
    }
}
