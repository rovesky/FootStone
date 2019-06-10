using System;
using System.Collections.Generic;
using System.Text;
using Orleans.Hosting;

namespace FootStone.Core
{
    public class FSHostBuilder : IFSHostBuilder
    {
      //  private SiloHostBuilder siloHostBuilder = new SiloHostBuilder();
        private readonly List<Action<ISiloHostBuilder>> configureSiloConfigActions = new List<Action<ISiloHostBuilder>>();
        private ISiloHost siloHost;
        private bool built = false;

        public FSHostBuilder()
        {
           
        }                


        public IFSHost Build()
        {
            if (this.built)
                throw new InvalidOperationException($"{nameof(this.Build)} can only be called once per {nameof(FSHostBuilder)} instance.");
            this.built = true;

            BuildSiloConfiguration();

            return new FSHost(siloHost);
        }

        private void BuildSiloConfiguration()
        {
            var siloHostBuilder = new SiloHostBuilder();
            foreach (var buildAction in this.configureSiloConfigActions)
            {
                buildAction(siloHostBuilder);
            }

            this.siloHost = siloHostBuilder.Build();
        }

        public IFSHostBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class
        {
            return this;
        }

        public IFSHostBuilder ConfigureSilo(Action<ISiloHostBuilder> configureSilo)
        {
            this.configureSiloConfigActions.Add(configureSilo ?? throw new ArgumentNullException(nameof(configureSilo)));
            return this;
        }
    }


}
