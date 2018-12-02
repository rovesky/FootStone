using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
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
        readonly PlayerPushPrx playerPush;
        public PlayerObserver(PlayerPushPrx playerPush)
        {
            this.playerPush = playerPush;
        }
        public void HpChanged(int hp)
        {
            playerPush.begin_hpChanged(hp);
        }
        
    }

    public class PlayerI : PlayerDisp_,IDisposable
    {
        private SessionI sessionI;
        private IPlayerObserver playerObserver;

        public PlayerI(SessionI session)
        {
            this.sessionI = session;           
        }


        private async Task AddObserver()
        {
            Console.Out.WriteLine("adding playerPush:" + sessionI.Name);
            PlayerPushPrx push = (PlayerPushPrx)PlayerPushPrxHelper.uncheckedCast(sessionI.SessionPushPrx, "playerPush").ice_oneway();
            playerObserver = await Global.Instance.OrleansClient.CreateObjectReference<IPlayerObserver>(new PlayerObserver(push));
            var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
            await player.SubscribeForPlayerUpdates(playerObserver );
        }

        public async override Task<PlayerInfo> GetPlayerInfoAsync(Current current = null)
        {
            try
            {
                if(sessionI.PlayerId == null)
                {
                    throw new PlayerNotExsit();
                }

                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
                var playerInfo = await player.GetPlayerInfo();
                await AddObserver();
                Console.Out.WriteLine("----------------GetPlayerInfo:" + playerInfo.name+"---------------------");
                return playerInfo;
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }

        public async override Task SetPlayerNameAsync(string name, Current current = null)
        {
            try
            {
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);

                await player.SetPlayerName(name);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public void Dispose()
        {
            if (playerObserver !=null)
            {
                Console.Out.WriteLine("playerObserver Unsubscribe");
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
                player.UnsubscribeForPlayerUpdates(playerObserver);
                playerObserver = null;
            }
            
        }
    }
}
