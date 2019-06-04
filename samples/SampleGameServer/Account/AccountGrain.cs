﻿using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using NLog;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    
    public class AccountState
    {
        public string account;
        public string password;
        public string curPlayerId;
        public string token;
        public Dictionary<int,List<PlayerShortInfo>> players;      
    }

    [StorageProvider(ProviderName = "memory1")]
    public class AccountGrain : FSGrain<AccountState,IAccountObserver>, IAccountGrain
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
  
        public AccountGrain()
        {        
            
        }


        public async override Task OnActivateAsync()
        {     

            await base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }



        public Task Login(string sessionId,string account,string pwd)
        {
            

         //   var info = JsonConvert.DeserializeObject<LoginInfo>(infoJson);
            if (State.account == null)
            {
                throw new AccountException("account is not registered!");
            }
            if(!(State.account.Equals(account)
                && State.password.Equals(pwd)))
            {
                throw new AccountException("account or password is not  valid!");
            }
            State.token = Guid.NewGuid().ToString();


            //通知所有session已经登录成功         
            Notify((s) =>
            {
                s.AccountLogined(sessionId);
            });
            return WriteStateAsync();
        }

        public Task Register(RegisterInfo info)
        {
            Console.WriteLine("Begin RegisterRequest:"+ info.account);
            if (State.account != null)
            {
                throw new AccountException("account is registered!");
            }

            State.account = info.account;
            State.password = info.password;

            return WriteStateAsync();
        }

        public async Task<string> CreatePlayer(string name, int gameId)
        {
            var playerId = Guid.NewGuid();
            var playerGrain = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.InitPlayer(name, gameId);

            if(State.players == null)
            {
                State.players = new Dictionary<int, List<PlayerShortInfo>>();               
            }
            if (!State.players.ContainsKey(gameId))
            {                
                State.players.Add(gameId, new List<PlayerShortInfo>());
            }
            State.players[gameId].Add(new PlayerShortInfo(playerId.ToString(), name,0,0));

            return playerId.ToString();
        }

        public Task<List<ServerInfo>> GetServerList()
        {
            var serveList = new List<ServerInfo>();
            serveList.Add(new ServerInfo(1, "server1", 0));
            return Task.FromResult(serveList);

        }

        public Task<List<PlayerShortInfo>> GetPlayerInfoShortList(int serverId)
        {
            if (State.players == null || !State.players.ContainsKey(serverId))
            {
                return Task.FromResult(new List<PlayerShortInfo>());
            }
            return Task.FromResult(State.players[serverId]);
        }

        public Task SelectPlayer(string playerId)
        {
            State.curPlayerId = playerId;
            return Task.CompletedTask;
        }

     
    }
}
