using DotNetty.Transport.Bootstrapping;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Client
{
    public class FSClientBuilder
    {
        private readonly List<Action<Bootstrap>> configureNettyActions = new List<Action<Bootstrap>>();
        private readonly List<Action<IceClientOptions>> configureIceActions = new List<Action<IceClientOptions>>();

        private bool built = false;
        private Bootstrap nettyBootstrap;
        private IceClientOptions iceClientOptions;

        public FSClientBuilder  NettyOptions(Action<Bootstrap> configureNetty)
        {
            this.configureNettyActions.Add(configureNetty ?? throw new ArgumentNullException(nameof(configureNetty)));
            return this;
        }

        public FSClientBuilder IceOptions(Action<IceClientOptions> configureIce)
        {
            this.configureIceActions.Add(configureIce ?? throw new ArgumentNullException(nameof(configureIce)));
            return this;
        }


        public IFSClient Build()
        {
            if (this.built)
                throw new InvalidOperationException($"{nameof(this.Build)} can only be called once per {nameof(FSClientBuilder)} instance.");
            this.built = true;
            BuildIceConfiguration();
            BuildNettyConfiguration();

            return new FSClient(iceClientOptions,nettyBootstrap);
        }

        private void BuildNettyConfiguration()
        {
            nettyBootstrap = new Bootstrap();
            foreach (var buildAction in this.configureNettyActions)
            {
                buildAction(nettyBootstrap);
            }         
        }

        private void BuildIceConfiguration()
        {
            iceClientOptions = new IceClientOptions();
            foreach (var buildAction in this.configureIceActions)
            {
                buildAction(iceClientOptions);
            }
        }
    }
}
