using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.Core
{
    public interface IFSClientBuilder
    {

        IFSClientBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class;

        IFSClientBuilder ConfigureOrleans(Action<IClientBuilder> configureSilo);    

        IFSClient Build();
        
    }
}