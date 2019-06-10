using Orleans.Hosting;
using System;

namespace FootStone.Core
{
    public interface IFSHostBuilder
    {

        IFSHostBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class;

        IFSHostBuilder ConfigureSilo(Action<ISiloHostBuilder> configureSilo);    

        IFSHost Build();

    }
}