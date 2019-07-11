using DotNetty.Transport.Channels;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public class FSSession : IFSSession
    {
        private string id;
        private SessionIce session;
        private IFSClient client;
        private IFSChannel channel;

        public bool IsEnableStream
        {
            get
            {
                return channel != null;
            }
        }

        public FSSession(string id, SessionIce session,IFSClient client)
        {       

            this.id = id;
            this.session = session;
            this.client = client;
 

            this.session.SessionPush.OnDestroyed += (sender, e) =>
            {
                OnDestroyed(sender, e);
            };
        }
     

        public event EventHandler OnDestroyed;

        public void Destory()
        {
            DestroyStreamChannel();

            session.SessionPrx.begin_Destroy();          
        }

     
        public string GetId()
        {
            return id;
        }

        public T UncheckedCast<T>(Func<ObjectPrx, string,T> uncheckedCast) where T : ObjectPrx
        {
            return uncheckedCast(session.SessionPrx, typeof(T).Name);
        }


        private string ParseHost(string endPoint)
        {
            var strs = endPoint.Split(' ');
            for (int i = 0; i < strs.Length; ++i)
            {
                if (strs[i] == "-h")
                {
                    return strs[i + 1];
                }
            }
            return "";
        }

        public async Task<IFSChannel> CreateStreamChannel()
        {
            var ip = ParseHost(session.SessionPrx.ice_getConnection().getEndpoint().ToString());
            channel = await client.CreateStreamChannel(ip, id);
            return channel;
        }

        public async void DestroyStreamChannel()
        {
            if (channel != null)
            {
                await channel.CloseAsync();
                channel = null;
            }
        }        
    }
}
