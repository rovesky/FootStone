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
        private readonly List<Action<Ice.InitializationData>> configureIceActions = new List<Action<Ice.InitializationData>>();

        private bool built = false;
        private Bootstrap nettyBootstrap;
        private InitializationData iceInitData;

        public FSClientBuilder  NettyOptions(Action<Bootstrap> configureNetty)
        {
            this.configureNettyActions.Add(configureNetty ?? throw new ArgumentNullException(nameof(configureNetty)));
            return this;
        }

        public FSClientBuilder IceOptions(Action<Ice.InitializationData> configureIce)
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

            return new FSClient(iceInitData,nettyBootstrap);
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
            iceInitData = new Ice.InitializationData();
            foreach (var buildAction in this.configureIceActions)
            {
                buildAction(iceInitData);
            }
        }
    }
}
