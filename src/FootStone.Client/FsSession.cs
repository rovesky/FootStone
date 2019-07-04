using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using FootStone.GrainInterfaces;
using Ice;

namespace FootStone.Client
{
    public class FSSession : IFSSession
    {

        private ISessionPrx session;
        private IChannel channel;

        public FSSession(ISessionPrx session,IChannel channel)
        {
            this.session = session;
            this.channel = channel;
        }
        public event EventHandler OnDestroyed;

        public void Destory()
        {
            session.begin_Destroy();
        }

        public ISessionPrx GetSessionPrx()
        {
            return session;
        }

        public IChannel GetChannel()
        {
            return channel;
        }
    }
}
