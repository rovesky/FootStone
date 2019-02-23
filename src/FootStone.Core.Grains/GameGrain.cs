using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Grains
{

    [StorageProvider(ProviderName = "memory1")]
    public  class GameGrain : Grain, IGameGrain,IPlayerObserver
    {
       

        private GameInfo gameInfo;

        private Dictionary<Guid, GamePlayerInfo> players = new Dictionary<Guid, GamePlayerInfo>();
        private Dictionary<Guid, IPlayerObserver[]> playersObserver = new Dictionary<Guid, IPlayerObserver[]>();
     //   private IPlayerObserver playerObserver;
        private IPlayerObserver[] observerArray = new IPlayerObserver[2];

        readonly IIceServiceClient IceServiceClient;

        public GameGrain(IGrainActivationContext grainActivationContext, IIceServiceClient iceServiceClient)
        {
            IceServiceClient = iceServiceClient;
        }


        public override Task OnActivateAsync()
        {
            long id = this.GetPrimaryKeyLong();
            gameInfo = new GameInfo(id);
            return Task.CompletedTask;
        }



        public override Task OnDeactivateAsync()
        {

            return Task.CompletedTask;
        }


        public virtual Task Init()
        {
            return Task.CompletedTask;
        }

        public virtual Task Fini()
        {
            return Task.CompletedTask;
        }



        public Task<GameInfo> GetGameInfo()
        {
            return Task.FromResult(gameInfo);
        }


        public Task EanbleGame()
        {
            throw new NotImplementedException();
        }
        public Task DisableGame()
        {
            throw new NotImplementedException();
        }

        public async Task PlayerEnter(GamePlayerInfo info)
        {
            Guid id = Guid.Parse(info.id);

            players.Add(id, info);

            //observerArray = new IPlayerObserver[2];
        //    playersObserver.Add(id, observerArray);         

            //observerArray[0] = new PlayerObserver(this, id);
          //  observerArray[1] = await this.GrainFactory.CreateObjectReference<IPlayerObserver>(observerArray[0]);

            var playerGrain = this.GrainFactory.GetGrain<IPlayerGrain>(id);
            await playerGrain.SubscribeForPlayerUpdates(this);
        }

        public async Task PlayerLeave(Guid playerId)
        {
           
            var playerGrain = this.GrainFactory.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.UnsubscribeForPlayerUpdates(this);
         //   playersObserver.Remove(playerId);

            players.Remove(playerId);

        }

        Task<List<GamePlayerInfo>> IGameGrain.GetOnlinePlayersByLevel(int level)
        {
            throw new NotImplementedException();
        }

        public void HpChanged(int hp)
        {
           
        }

        public void LevelChanged(Guid playerId, int newLevel)
        {
            var info = this.players[playerId];
            info.level = newLevel;
            Console.WriteLine($"{info.name} new level {info.level}");
        }

        //class PlayerObserver : IPlayerObserver
        //{
        //    private GameGrain gameGrain;
        //    private Guid id;

        //    public PlayerObserver(GameGrain gameGrain, Guid id)
        //    {
        //        this.gameGrain = gameGrain;
        //        this.id = id;
        //    }

        //    public void HpChanged(int hp)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void LevelChanged(int oldLevel, int newLevel)
        //    {
        //        var info = gameGrain.players[id];
        //        info.level = newLevel;
        //    }
        //}
    }
}
