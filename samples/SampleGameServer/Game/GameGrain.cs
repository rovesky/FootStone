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

    [StorageProvider(ProviderName = "ado1")]
    public  partial class GameGrain :  FSGrain<GameState>
    {
        public GameGrain()
        {

        }

        public override  Task OnActivateAsync()
        {
            AddComponent(new GameComponent<GameState>(this,State));
            AddComponent(new PlayerManagerComponent(this));

            return base.OnActivateAsync();
        }


    //    protected abstract IFSComponent CreateGameComponent();
        

    }
}