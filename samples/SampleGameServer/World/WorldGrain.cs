using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Grains
{

    public class WorldGrain : FSGrain, IWorldGrain
    {

        class PlayerObserver : IPlayerObserver
        {
            public void HpChanged(int hp)
            {
                
            }

            public void LevelChanged(Guid playerId, int newLevel)
            {
               
            }
        }
        private List<GameState> gameInfos = new List<GameState>();


        //public override async Task OnActivateAsync()
        //{
        //    //try {
        //    //    var obj = await Global.OrleansClient.CreateObjectReference<IPlayerObserver>(new PlayerObserver());
        //    //}
        //    //catch ( Exception e)
        //    //{
        //    //    Console.WriteLine(e.StackTrace);
        //    //}

        //    //  return Task.CompletedTask;
        //    base.OnActivateAsync
        //}

        //public override Task OnDeactivateAsync()
        //{

        //    return Task.CompletedTask;
        //}




        public async Task AddGame(GameState gameInfo)
        {
            gameInfos.Add(gameInfo);
            if (gameInfo.enabled)
            {
                await EnableGame(gameInfo.id);
            }
        }


        public Task<List<GameState>> GetGameInfoList()
        {
            return Task.FromResult(gameInfos);
        }

        public async Task EnableGame(long id)
        {
            IGameGrain game = GrainFactory.GetGrain<IGameGrain>(id);

            await game.EanbleGame();
        }

        public async Task DisableGame(long id)
        {
            IGameGrain game = GrainFactory.GetGrain<IGameGrain>(id);

            await game.DisableGame();
        }

        public async Task DisableAllGames()
        {

            foreach (GameState gameInfo in gameInfos)
            {
                if (gameInfo.enabled)
                {
                    IGameGrain game = GrainFactory.GetGrain<IGameGrain>(gameInfo.id);
                    await game.DisableGame();
                }
            }
        }

        public Task UpdateGameInfo(GameState info)
        {
            return Task.CompletedTask;
        }

        public async Task Init(string configRoot)
        {


            using (var jsonStream = new JsonTextReader(File.OpenText($"{configRoot}\\GameData\\Games.json")))
            {
                var deserializer = new JsonSerializer();
                var gameConfigs = deserializer.Deserialize<List<GameConfig>>(jsonStream);

                foreach (var config in gameConfigs)
                {
                    var gameInfo = new GameState(config.id);
                    gameInfo.name = config.name;
                  
                    await AddGame(gameInfo);
                }
            }        
            

        }


        public Task<List<ServerInfo>> GetServerList()
        {
            var serveList = new List<ServerInfo>();
            serveList.Add(new ServerInfo(1, "server1", 0));
            return Task.FromResult(serveList);

        }

        public async Task<List<PlayerShortInfo>> GetPlayerInfoShortList(string account, int gameId)
        {
            //IGameGrain gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);

            //var gamePlayers = await gameGrain.GetPlayersByAccount(account);
            //var rets = new List<PlayerShortInfo>();
            //foreach (var gamePlayer in gamePlayers)
            //{
            //    rets.Add(new PlayerShortInfo(gamePlayer.id, gamePlayer.name, 1, 0));

            //}

            IAccountGrain accountGrain = GrainFactory.GetGrain<IAccountGrain>(account);
            return await accountGrain.GetPlayersShortInfo(gameId);
        }
    }
  
}
