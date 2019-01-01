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
        readonly IPlayerPushPrx playerPush;
        public PlayerObserver(IPlayerPushPrx playerPush)
        {
            this.playerPush = playerPush;
        }
        public void HpChanged(int hp)
        {
            //try
            //{
             //   Console.Out.WriteLine("hp changed:"+hp);
                playerPush.begin_hpChanged(hp);
            //}
            //catch(Ice.Exception e)
            //{
            //    Console.Error.WriteLine(e.Message);
            //}
         
        }
        
    }

    public class PlayerI : IPlayerDisp_, IServantBase
    {
        private SessionI sessionI;
        private IPlayerObserver playerObserver;
        private IPlayerObserver playerObserverRef;

        public PlayerI(SessionI session)
        {
            this.sessionI = session;           
        }


        public async Task AddObserver()
        {
            if (playerObserver == null)
            {
                Console.Out.WriteLine("add playerPush:" + sessionI.Account);
                IPlayerPushPrx push = (IPlayerPushPrx)IPlayerPushPrxHelper.uncheckedCast(sessionI.SessionPushPrx, "playerPush").ice_oneway();
                playerObserver = new PlayerObserver(push);
                playerObserverRef = await Global.OrleansClient.CreateObjectReference<IPlayerObserver>(playerObserver);
                var playerGrain = Global.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
                await playerGrain.SubscribeForPlayerUpdates(playerObserverRef);
            }
        }

        public async override Task<PlayerInfo> GetPlayerInfoAsync(Current current = null)
        {
            try
            {
                if(sessionI.PlayerId == null)
                {
                    throw new PlayerNotExsit();
                }

                var player = Global.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
                var playerInfo = await player.GetPlayerInfo();
       
           //     Console.Out.WriteLine("----------------GetPlayerInfo:" + playerInfo.name+"---------------------");
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
                var player = Global.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);

                await player.SetPlayerName(name);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }


        public void Destroy()
        {
            if (playerObserver != null)
            {
                Console.Out.WriteLine("playerObserver Unsubscribe");
                var player = Global.OrleansClient.GetGrain<IPlayerGrain>(sessionI.PlayerId);
                player.UnsubscribeForPlayerUpdates(playerObserverRef);
                playerObserver = null;
            }
        }
    }
}
