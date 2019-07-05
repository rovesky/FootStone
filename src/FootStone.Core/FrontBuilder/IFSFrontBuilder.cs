using Orleans;
using Orleans.Hosting;
using System;

namespace FootStone.Core
{
    public interface IFSFrontBuilder
    {

        IFSFrontBuilder Configure<TOptions>(Action<TOptions> configureOptions) where TOptions : class;

        IFSFrontBuilder ConfigureOrleans(Action<IClientBuilder> configureSilo);    

        IFSFront Build();
        
    }
}