using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using NLog;
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

        private NLog.Logger logger = NLog.LogManager.GetLogger("FootStone.Grains.WorldGrain");
        private List<GameState> gameInfos = new List<GameState>();
      //  private List<ServerInfo> serveList = new List<ServerInfo>();

        class PlayerObserver : IPlayerObserver
        {
            public void HpChanged(int hp)
            {
                
            }

            public void LevelChanged(Guid playerId, int newLevel)
            {
               
            }
        }       
       
        private static string Pad(int value, int width)
        {
            return value.ToString("d").PadRight(width);
        }


        public override async Task OnActivateAsync()
        {
            //   serveList.Add(new ServerInfo(1, "server1", 0));
            await this.Init(System.Environment.CurrentDirectory);

            RegisterTimer(async (s1) =>
            {
                logger.Warn("Server Name            Id           Player Count");
                logger.Warn("---------------------  -----------  ------------");
                foreach (var s in gameInfos)
                {
                    IGameGrain gameGrain = GrainFactory.GetGrain<IGameGrain>(s.id);
                    var count = await gameGrain.GetPlayerCount();
                    logger.Warn("{0}  {1}  {2}", s.name.ToString().PadRight(21), Pad((int)s.id, 11), count);
                }

                var managerGrain = GrainFactory.GetGrain<IManagementGrain>(0);
                var stats = await managerGrain.GetSimpleGrainStatistics();
                logger.Warn("Silo                   Activations  Type");
                logger.Warn("---------------------  -----------  ------------");
                foreach (var s in stats.OrderBy(s => s.SiloAddress + s.GrainType))
                    logger.Warn("{0}  {1}  {2}", s.SiloAddress.ToString().PadRight(21), Pad(s.ActivationCount, 11), s.GrainType);

            }
            , null
            , TimeSpan.FromSeconds(10)
            , TimeSpan.FromSeconds(10));

            await base.OnActivateAsync();
        }

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
                    gameInfo.enabled = true;

                    await AddGame(gameInfo);
                }
            }
        }

        public Task<List<ServerInfo>> GetServerList()
        {
            List<ServerInfo> servers = new List<ServerInfo>();

            foreach(var game in gameInfos)
            {
                servers.Add(new ServerInfo((int)game.id, game.name, 0));
            }

            return Task.FromResult(servers);
        }

        public async Task<List<PlayerShortInfo>> GetPlayerInfoShortList(string account, int gameId)
        {  
            IAccountGrain accountGrain = GrainFactory.GetGrain<IAccountGrain>(account);
            return await accountGrain.GetPlayersShortInfo(gameId);
        }
    }  
}
