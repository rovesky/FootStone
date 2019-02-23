using System;
using System.Threading.Tasks;

namespace SampleGrainInterfaces
{
    public interface ISampleGameGrain
    {
        Task SampleBattleBegin();

        Task SampleBattleEnd();

    }
}
