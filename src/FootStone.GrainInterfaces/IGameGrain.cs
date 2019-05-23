using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IGameGrain : 
        IGameManager,
        IPlayerManager,
        IGrainWithIntegerKey     

    { 

      
    }
}
