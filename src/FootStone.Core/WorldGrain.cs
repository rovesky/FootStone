using FootStone.Core;
using FootStone.Core.GrainInterfaces;
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

    public class WorldGrain : FootStoneGrain, IWorldGrain
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
        private List<GameInfo> gameInfos = new List<GameInfo>();


        public override async Task OnActivateAsync()
        {
            //try {
            //    var obj = await Global.OrleansClient.CreateObjectReference<IPlayerObserver>(new PlayerObserver());
            //}
            //catch ( Exception e)
            //{
            //    Console.WriteLine(e.StackTrace);
            //}

            //  return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {

            return Task.CompletedTask;
        }




        public async Task AddGame(GameInfo gameInfo)
        {
            gameInfos.Add(gameInfo);
            if (gameInfo.enabled)
            {
                await EnableGame(gameInfo.id);
            }
        }


        public Task<List<GameInfo>> GetGameInfoList()
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

            foreach (GameInfo gameInfo in gameInfos)
            {
                if (gameInfo.enabled)
                {
                    IGameGrain game = GrainFactory.GetGrain<IGameGrain>(gameInfo.id);
                    await game.DisableGame();
                }
            }
        }

        public Task UpdateGameInfo(GameInfo info)
        {
            return Task.CompletedTask;
        }

        public async Task Init(string configRoot)
        {


            using (var jsonStream = new JsonTextReader(File.OpenText($"{configRoot}\\Games.json")))
            {
                var deserializer = new JsonSerializer();
                var gameConfigs = deserializer.Deserialize<List<GameConfig>>(jsonStream);

                foreach (var config in gameConfigs)
                {
                    var gameInfo = new GameInfo(config.id);
                    gameInfo.name = config.name;
                  
                    await AddGame(gameInfo);
                }
            }        
            

        }
    }
  
}
