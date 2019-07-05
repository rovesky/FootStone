using DotNetty.Transport.Bootstrapping;
using System;
using System.Collections.Generic;

namespace FootStone.Client
{
    public class FSClientBuilder
    {
        private readonly List<Action<NettyClientOptions>> configureNettyActions = new List<Action<NettyClientOptions>>();
        private readonly List<Action<IceClientOptions>> configureIceActions = new List<Action<IceClientOptions>>();

        private bool built = false;
        private NettyClientOptions nettyOptions;
        private IceClientOptions   iceOptions;

        public FSClientBuilder  NettyOptions(Action<NettyClientOptions> configureNetty)
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

            return new FSClient(iceOptions,nettyOptions);
        }

        private void BuildNettyConfiguration()
        {
            nettyOptions = new NettyClientOptions();
            foreach (var buildAction in this.configureNettyActions)
            {
                buildAction(nettyOptions);
            }         
        }

        private void BuildIceConfiguration()
        {
            iceOptions = new IceClientOptions();
            foreach (var buildAction in this.configureIceActions)
            {
                buildAction(iceOptions);
            }
        }
    }
}
