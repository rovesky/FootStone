using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GrainInterfaces
{
    public interface IAccountObserver : IGrainObserver
    {
        void AccountLogined(string sessionId);
    }
}
