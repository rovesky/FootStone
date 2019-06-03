using FootStone.Core.GrainInterfaces;
using FootStone.Core;
using Orleans;
using SampleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FootStone.Game;

namespace SampleGameServer
{
    public class SampleGameGrain : GameGrain,ISampleGameGrain
    {
       

        public override Task OnActivateAsync()
        {          
            var ret =  base.OnActivateAsync();

            AddComponent(new SampleBattleComponent(this));
            return ret;
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

        protected override IComponent CreateGameComponent()
        {
            return new GameComponent<SampleGameState>(this,new SampleGameState(1));
        }
    }

    internal class SampleGameState : GameState
    {
        public string sampleName;

        public SampleGameState(long id) :base(id)
        {
            
        }
    }
}
