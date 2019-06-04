using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Game
{
    public interface IFSGrain
    {
        IGrainFactory GrainFactory { get; }
    }

}
