using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{

    [StorageProvider(ProviderName = "memory1")]
    public abstract partial class GameGrain :  FSGrain
    {

        public override  Task OnActivateAsync()
        {
            AddComponent(CreateGameComponent());
            AddComponent(new PlayerManagerComponent(this));

            return base.OnActivateAsync();
        }


        protected abstract IFSComponent CreateGameComponent();
        

    }
}