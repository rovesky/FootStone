using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{

    /// <summary>
    /// Observer class that implements the observer interface. Need to pass a grain reference to an instance of this class to subscribe for updates.
    /// </summary>
    class PlayerObserver : IPlayerObserver
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        readonly IPlayerPushPrx playerPush;
        public PlayerObserver(IPlayerPushPrx playerPush)
        {
            this.playerPush = playerPush;
        }
        public void HpChanged(int hp)
        {
            logger.Debug("HpChanged:" + hp);
            playerPush.begin_hpChanged(hp);
        }

        public void LevelChanged(Guid id, int newLevel)
        {

        }
    }

    public class PlayerI : IPlayerDisp_, IServantBase
    {
        private SessionI session;

        private ObserverClient<IPlayerObserver> observer = new ObserverClient<IPlayerObserver>(Global.OrleansClient);
        private IPlayerGrain playerGrain;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public PlayerI()
        {
           
        }

        public string GetFacet()
        {
            return "player";
        }

        public void setSessionI(SessionI sessionI)
        {
            this.session = sessionI;
        }
             

        public async override Task<string> CreatePlayerRequestAsync(int gameId, PlayerCreateInfo info,Current current = null)
        {
            var playerId = Guid.NewGuid();
            var playerGrain = Global.OrleansClient.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.CreatePlayer(session.Account, gameId, info);
            return playerId.ToString();
        }

        public async override Task SelectPlayerRequestAsync(string playerId, Current current = null)
        {
            var gpid = Guid.Parse(playerId);
            playerGrain = Global.OrleansClient.GetGrain<IPlayerGrain>(gpid);

            await observer.Subscribe(playerGrain, new PlayerObserver(IPlayerPushPrxHelper.uncheckedCast(session.SessionPushPrx,"playerPush")));

            await playerGrain.PlayerOnline();

            session.PlayerId = gpid;

            logger.Debug($"Bind Session ${session.Account}:${session.PlayerId}");
        }

        public async override Task<PlayerInfo> GetPlayerInfoAsync(Current current = null)
        {        
            return await playerGrain.GetPlayerInfo();           
        }

        public async override Task SetPlayerNameAsync(string name, Current current = null)
        {       

            await playerGrain.SetPlayerName(name);
        }


        public async void Dispose()
        {
            await observer.Unsubscribe();
        }

    }
}
