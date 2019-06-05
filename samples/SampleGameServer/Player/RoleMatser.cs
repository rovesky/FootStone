using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;

namespace FootStone.Core
{

    public partial class PlayerGrain : FSGrain<PlayerInfo, IPlayerObserver>, IRoleMasterGrain
    {
        public Task<MasterProperty> GetProperty()
        {
            return Task.FromResult(State.roleMaster.property);
        }

        public Task SetProperty(MasterProperty property)
        {
            State.roleMaster.property = property;
            return Task.CompletedTask;
        }
    }
}
