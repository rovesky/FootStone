using System;
using System.Collections.Generic;
using System.Text;
using FootStone.Core.GrainInterfaces;

namespace SampleGrainInterfaces
{
    public interface ISampleGameGrain : IGameGrain,ISampleBattle
    {
    }
}
