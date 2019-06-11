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

    [StorageProvider(ProviderName= "memory1")]
    public partial class PlayerGrain : FSGrain<PlayerInfo, IPlayerObserver>, IPlayerGrain
    {    
        //private IZoneGrain zoneGrain;
        //private bool isOnline;

        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IDisposable timer;

        public PlayerGrain(IGrainActivationContext grainActivationContext)
        {
        }

        //public override Task OnActivateAsync()
        //{                      
        //    return Task.CompletedTask;
        //}

        //public override Task OnDeactivateAsync()
        //{
        //    return Task.CompletedTask;
        //}
                  

        public  Task<PlayerInfo> GetPlayerInfo()
        {
            return Task.FromResult(this.State);
        }
               

        public  Task SetPlayerName(string name)
        {
            this.State.name = name;      
            return Task.CompletedTask;
           // await WriteStateAsync();
        }

        public async Task CreatePlayer(string account, int gameId,PlayerCreateInfo info)
        {
            this.State.account = account;
            this.State.playerId = this.GetPrimaryKey().ToString();
            this.State.name = info.name;
            this.State.gameId = gameId;

            this.State.zoneId = Guid.NewGuid().ToString();
            this.State.items = new List<Item>();
            this.State.items.Add(new Item("1", "item1", 1));
            this.State.items.Add(new Item("2", "item2", 2));
            this.State.roleMaster.property.intel = 10;
            this.State.roleMaster.property.str = 10;
            this.State.roleMaster.property.agil = 10;
            logger.Debug("create player:" + this.State.name);
            await WriteStateAsync();
        }

        public async Task PlayerOnline()
        {

            logger.Debug($"{State.account} PlayerOnline!");
            IGameGrain gameGrain = this.GrainFactory.GetGrain<IGameGrain>(State.gameId);

            var gamePlayerInfo = new GamePlayerState();
            gamePlayerInfo.id = this.State.playerId;
            gamePlayerInfo.name = this.State.name;
            gamePlayerInfo.level = this.State.level;
            await gameGrain.PlayerEnter(gamePlayerInfo);

            IAccountGrain accountGrain = this.GrainFactory.GetGrain<IAccountGrain>(State.account);
            await accountGrain.setCurPlayerId(State.playerId);

            timer = RegisterTimer((s) =>
            {
              //  logger.Debug($"{State.account} timer triger!");

                State.roleMaster.property.intel++;
                State.level++;
                Notify((t) =>
                {
                    t.HpChanged(State.roleMaster.property.intel);
                    t.LevelChanged(Guid.Parse(State.playerId), this.State.level);
                });

                WriteStateAsync();
                return Task.CompletedTask;
            }
               , null
               , TimeSpan.FromSeconds(10)
               , TimeSpan.FromSeconds(10));
        }

        public async Task PlayerOffline()
        {

            logger.Debug($"{State.account} PlayerOffline!");
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            IGameGrain gameGrain = this.GrainFactory.GetGrain<IGameGrain>(State.gameId);
            await gameGrain.PlayerLeave(Guid.Parse(State.playerId));

            IAccountGrain accountGrain = this.GrainFactory.GetGrain<IAccountGrain>(State.account);
            await accountGrain.setCurPlayerId("");
        }
    }
}
