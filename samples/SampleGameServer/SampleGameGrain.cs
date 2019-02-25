using FootStone.Core.GrainInterfaces;
using FootStone.Core;
using Orleans;
using SampleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public class SampleGameGrain : GameGrain,ISampleGameGrain
    {

        public override Task OnActivateAsync()
        {
            AddComponent(new SampleBattleComponent(this));
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        public Task SampleBattleBegin()
        {
            return FindComponent<ISampleBattle>().SampleBattleBegin();
        }

        public Task SampleBattleEnd()
        {
            return FindComponent<ISampleBattle>().SampleBattleEnd();
        }
    }
}
