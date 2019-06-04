using FootStone.Core;
using FootStone.Game;
using SampleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public class SampleBattleComponent : ComponentBase,ISampleBattle
    {
        
        public SampleBattleComponent(FSGrain grain) : base(grain)
        {

        }

        public override Task Fini()
        {
            return Task.CompletedTask;
        }

        public override Task Init()
        {
            return Task.CompletedTask;
        }

        public Task SampleBattleBegin()
        {
            Console.WriteLine("SampleBattleBegin");
            return Task.CompletedTask;
        }

        public Task SampleBattleEnd()
        {
            Console.WriteLine("SampleBattleBegin");
            return Task.CompletedTask;
        }
    }
}
