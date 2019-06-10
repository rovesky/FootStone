using System;
using System.Collections.Generic;
using System.Text;
using Orleans;
using Orleans.Hosting;

namespace FootStone.Core
{
    public class FSClientBuilder : IFSClientBuilder
    {
      //  private SiloHostBuilder siloHostBuilder = new SiloHostBuilder();
        private readonly List<Action<IClientBuilder>> configureSiloConfigActions = new List<Action<IClientBuilder>>();
        private IClusterClient clusterClient;
        private bool built = false;

        public FSClientBuilder()
        {
           
        }                


        public IFSClient Build()
        {
            if (this.built)
                throw new InvalidOperationException($"{nameof(this.Build)} can only be called once per {nameof(FSHostBuilder)} instance.");
            this.built = true;

            BuildSiloConfiguration();

            return new FSClient(clusterClient);
        }

        private void BuildSiloConfiguration()
        {
            var clientBuilder = new ClientBuilder();
            foreach (var buildAction in this.configureSiloConfigActions)
            {
                buildAction(clientBuilder);
            }

            this.clusterClient = clientBuilder.Build();
        }

        public IFSClientBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class
        {
            return this;
        }

     
        public IFSClientBuilder ConfigureOrleans(Action<IClientBuilder> configureSilo)
        {
            this.configureSiloConfigActions.Add(configureSilo ?? throw new ArgumentNullException(nameof(configureSilo)));
            return this;
        }
    }


}
