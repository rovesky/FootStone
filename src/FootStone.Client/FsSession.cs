using DotNetty.Transport.Channels;
using FootStone.GrainInterfaces;
using System;

namespace FootStone.Client
{
    public class FSSession : IFSSession
    {
        private string id;
        private SessionIce session;
        private IChannel channel;

        public FSSession(string id, SessionIce session,IChannel channel)
        {
            this.id = id;
            this.session = session;
            this.channel = channel;

            this.session.SessionPush.OnDestroyed += (sender, e) =>
            {
                OnDestroyed(sender, e);
            };
        }
     

        public event EventHandler OnDestroyed;

        public void Destory()
        {
            session.SessionPrx.begin_Destroy();
        }

        public ISessionPrx GetSessionPrx()
        {
            return session.SessionPrx;
        }

        public IChannel GetStreamChannel()
        { 
            return channel;
        }

        public string GetId()
        {
            return id;
        }
    }
}
