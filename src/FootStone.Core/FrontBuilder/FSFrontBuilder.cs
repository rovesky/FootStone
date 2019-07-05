using System;
using System.Collections.Generic;
using System.Text;
using Orleans;
using Orleans.Hosting;

namespace FootStone.Core
{
    public class FSFrontBuilder : IFSFrontBuilder
    {

        private readonly List<Action<IClientBuilder>> configureSiloConfigActions = new List<Action<IClientBuilder>>();
        private IClusterClient clusterClient;
        private bool built = false;

        public FSFrontBuilder()
        {
           
        }                


        public IFSFront Build()
        {
            if (this.built)
                throw new InvalidOperationException($"{nameof(this.Build)} can only be called once per {nameof(FSHostBuilder)} instance.");
            this.built = true;

            BuildSiloConfiguration();

            Global.OrleansClient = clusterClient;

            return new FSFront(clusterClient);
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

        public IFSFrontBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class
        {
            return this;
        }

     
        public IFSFrontBuilder ConfigureOrleans(Action<IClientBuilder> configureSilo)
        {
            this.configureSiloConfigActions.Add(configureSilo ?? throw new ArgumentNullException(nameof(configureSilo)));
            return this;
        }
    }
}
