using FootStone.Grains;
using Orleans;
using SampleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public class SampleGameGrain : GameGrain, ISampleGameGrain
    {

        public override Task OnActivateAsync()
        {
            long id = this.GetPrimaryKeyLong();

            Console.WriteLine($"SampleGameGrain({id}) OnActivateAsync!");
            return base.OnActivateAsync();
        }



        public override Task OnDeactivateAsync()
        {

             return base.OnDeactivateAsync();
        }
        public Task SampleBattleBegin()
        {
            throw new NotImplementedException();
        }

        public Task SampleBattleEnd()
        {
            throw new NotImplementedException();
        }
    }
}
