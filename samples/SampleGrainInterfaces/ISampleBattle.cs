using System;
using System.Threading.Tasks;

namespace SampleGrainInterfaces
{
    public interface ISampleBattle 
    {
        Task SampleBattleBegin();

        Task SampleBattleEnd();

    }
}
